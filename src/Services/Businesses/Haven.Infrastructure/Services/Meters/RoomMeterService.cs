namespace Haven.Infrastructure.Services.Meters;

/// <summary>
/// Coordinates room-scoped meter history, monthly values, evidence, and invoice synchronisation.
/// </summary>
public sealed class RoomMeterService : IRoomMeterService
{
    private readonly IMeterRepository _meterRepository;
    private readonly IInvoiceUtilityRepository _invoiceUtilityRepository;
    private readonly IMasterDataService _masterDataService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLockService _distributedLockService;
    private readonly IObjectStorageService _objectStorageService;
    private readonly R2StorageOptions _r2StorageOptions;
    private readonly ILogger<RoomMeterService> _logger;

    /// <summary>
    /// Creates the room meter workflow service.
    /// </summary>
    /// <param name="repositories">The repositories for room meters and invoice utility persistence.</param>
    /// <param name="masterDataService">The exact master-data resolver used by utility mutations.</param>
    /// <param name="unitOfWork">The transaction boundary shared by meters, evidence, and invoices.</param>
    /// <param name="distributedLockService">The lock service that serializes one room and month.</param>
    /// <param name="objectStorageService">The object-storage service used for post-transaction clean-up.</param>
    /// <param name="storageOptions">The configured meter object prefix and public URL.</param>
    /// <param name="logger">The structured utility workflow logger.</param>
    public RoomMeterService(
        MeterRepositoryDependencies repositories,
        IMasterDataService masterDataService,
        IUnitOfWork unitOfWork,
        IDistributedLockService distributedLockService,
        IObjectStorageService objectStorageService,
        IOptions<R2StorageOptions> storageOptions,
        ILogger<RoomMeterService> logger)
    {
        _meterRepository = repositories.MeterRepository;
        _invoiceUtilityRepository = repositories.InvoiceUtilityRepository;
        _masterDataService = masterDataService;
        _unitOfWork = unitOfWork;
        _distributedLockService = distributedLockService;
        _objectStorageService = objectStorageService;
        _r2StorageOptions = storageOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Loads one room meter month together with the immediately preceding month.
    /// </summary>
    /// <param name="request">The landlord-scoped room, month, and year.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The selected period, previous period, evidence, and live invoice impact.</returns>
    public async Task<MeterPeriodDetailResponseDto> GetPeriodAsync(
        MeterPeriodRequestModel request,
        CancellationToken cancellationToken = default)
    {
        var periods = MeterPeriodsModel.From(request.Year, request.Month);

        // Resolve the public room once so every subsequent query uses internal BIGINT identifiers.
        var scope = await GetScopeAsync(request.RoomId, request.CurrentParty.PartyId, cancellationToken);
        var parameters = new MeterPeriodQueryParametersModel
        {
            CurrentPartyId = request.CurrentParty.PartyId,
            PropertyId = scope.PropertyId,
            UnitId = scope.UnitId,
            PeriodFrom = periods.CurrentFrom,
            PeriodTo = periods.CurrentTo,
            PreviousPeriodFrom = periods.PreviousFrom,
            PreviousPeriodTo = periods.PreviousTo,
            RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
            ConfirmedMeterStatusCode = MASTER_CODE_METER_STATUS_CONFIRMED
        };

        // Read both periods in one typed projection, then attach evidence without materialising an EF graph.
        var rows = await _meterRepository.GetPeriodRowsAsync(parameters, cancellationToken);
        var evidence = await LoadEvidenceAsync(rows, cancellationToken);
        var response = MeterResponseMapper.MapPeriodDetail(
            scope,
            periods.CurrentFrom,
            periods.PreviousFrom,
            rows,
            evidence);

        // Invoice impact is advisory for the UI; mutation still rechecks live invoice state in its transaction.
        response.InvoiceImpact = await GetInvoiceImpactAsync(scope.UnitId, periods, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.MeterLogs.PERIOD_LOADED,
            $"{request.Month:D2}-{request.Year}",
            scope.PropertyPublicId,
            scope.RoomPublicId);

        return response;
    }

    /// <summary>
    /// Loads at most twelve meter periods for one room and calendar year.
    /// </summary>
    /// <param name="request">The landlord-scoped room and year.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The monthly history ordered from newest to oldest.</returns>
    public async Task<IReadOnlyList<MeterHistoryItemResponseDto>> GetHistoryAsync(
        MeterHistoryRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Resolve room ownership once before using internal IDs in the bounded history query.
        var scope = await GetScopeAsync(request.RoomId, request.CurrentParty.PartyId, cancellationToken);
        var parameters = new MeterPeriodQueryParametersModel
        {
            CurrentPartyId = request.CurrentParty.PartyId,
            PropertyId = scope.PropertyId,
            UnitId = scope.UnitId,
            HistoryFrom = new DateOnly(request.Year, 1, 1),
            HistoryTo = new DateOnly(request.Year, 12, 31),
            RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
            ConfirmedMeterStatusCode = MASTER_CODE_METER_STATUS_CONFIRMED
        };

        // The Dapper projection returns flat rows; this layer groups electricity and water into one month card.
        var rows = await _meterRepository.GetHistoryRowsAsync(parameters, cancellationToken);
        var evidence = await LoadEvidenceAsync(rows, cancellationToken);
        var items = rows
            .GroupBy(row => row.BillingPeriodFrom)
            .Select(group => MeterResponseMapper.MapHistoryItem(group.Key, group.ToList(), evidence))
            .ToList();

        return items;
    }

    /// <summary>
    /// Saves and confirms one complete room meter month using the requested existence rule.
    /// </summary>
    /// <param name="request">The flat multipart meter values, prices, date, and optional evidence.</param>
    /// <param name="mode">The create or update mode enforced after the room lock is acquired.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The committed period response.</returns>
    public async Task<MeterPeriodDetailResponseDto> SaveConfirmedPeriodAsync(
        MeterMutationRequestModel request,
        MeterMutationModeEnum mode,
        CancellationToken cancellationToken = default)
    {
        // Application has already validated the typed date; derive one reusable calendar-period invariant from it.
        var billingDate = DateOnly.FromDateTime(request.BillingDate);
        var periods = MeterPeriodsModel.From(request.BillingDate);
        var lockKey = $"{METER_LOCK_KEY_PREFIX}{request.RoomId}:{billingDate:yyyy-MM}";

        // Resolve read scope before upload; the transaction rechecks and locks the same room before persistence.
        var uploadScope = await GetScopeAsync(
            request.RoomId,
            request.CurrentParty.PartyId,
            cancellationToken);

        // Upload validated image additions before acquiring the distributed lock so storage I/O does not extend the protected period mutation.
        var uploads = await UploadEvidenceAsync(
            uploadScope.RoomPublicId,
            request.ElectricImages,
            request.WaterImages,
            cancellationToken);
        var hasEvidenceChanges = uploads.Count > 0
                                 || request.DeletedElectricImageIds.Count > 0
                                 || request.DeletedWaterImageIds.Count > 0;
        var replacedObjectKeys = new List<string>();
        var committed = false;

        try
        {
            // Serialize the room-period mutation only after external uploads have completed.
            await using var lockHandle = await _distributedLockService.TryAcquireAsync(
                lockKey,
                TimeSpan.FromSeconds(METER_LOCK_LEASE_SECONDS),
                cancellationToken) ?? throw new ApiException(
                    ApplicationErrorConstants.MeterErrors.ERROR_METER_LOCKED,
                    BAD_REQUEST);

            // Resolve only the master-data values required by this write path before entering the transaction.
            var lookups = await ResolveMutationLookupsAsync(hasEvidenceChanges, cancellationToken);

            await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                // Prepare scope, tracked rows, policies, and baselines through EF on the active transaction connection.
                var state = await _meterRepository.GetMutationStateAsync(
                    new MeterMutationStateRequestModel
                    {
                        RoomPublicId = request.RoomId,
                        CurrentPartyId = request.CurrentParty.PartyId,
                        RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES,
                        PeriodFrom = periods.CurrentFrom,
                        PeriodTo = periods.CurrentTo,
                        ConfirmedStatusId = lookups.ConfirmedMeterStatus.Id,
                        ElectricLineTypeId = lookups.ElectricLineType.Id,
                        WaterLineTypeId = lookups.WaterLineType.Id
                    },
                    transactionToken) ?? throw new ApiException(
                        ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                        NOT_FOUND,
                        StatusCodes.Status404NotFound);

                // Enforce create/update semantics only after the locked database state is available.
                GuardMutationMode(mode, state.HasExistingPeriod);

                // Effective METER_READING policies define the complete utility set that must be confirmed together.
                var context = new MeterMutationContextModel(
                    request,
                    state,
                    lookups,
                    periods,
                    billingDate,
                    transactionToken);

                // Apply both required meter policies before saving, so a room never commits one half of a meter period.
                await ApplyUtilityAsync(
                    MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC,
                    state.Electric,
                    context);
                await ApplyUtilityAsync(
                    MASTER_CODE_INVOICE_LINE_TYPE_WATER,
                    state.Water,
                    context);

                // Persist meter records first so evidence links can reference database identifiers in the same transaction.
                await _unitOfWork.SaveChangesAsync(transactionToken);

                // Add selected image uploads and retire only explicitly selected evidence after meter IDs exist in the same transaction.
                await PersistEvidenceAsync(
                    uploads,
                    request.DeletedElectricImageIds,
                    request.DeletedWaterImageIds,
                    context,
                    replacedObjectKeys,
                    transactionToken);

                // Read and mutate invoice state only after the room lock is held; no GET warning authorises this write.
                await SynchronizeInvoiceAsync(
                    state.Scope,
                    periods,
                    lookups,
                    state.GetMeters(),
                    transactionToken);
            }, cancellationToken);

            committed = true;

            // Old objects are removed only after commit; clean-up failure must not turn a committed mutation into an API error.
            using var replacementCleanupCts = new CancellationTokenSource(OBJECT_STORAGE_CLEANUP_TIMEOUT);

            try
            {
                var cleanup = await ObjectStorageHelper.CleanupObjectKeysAsync(
                    _objectStorageService,
                    replacedObjectKeys,
                    replacementCleanupCts.Token);

                if (cleanup.HasFailures)
                {
                    _logger.LogWarning(
                        InfrastructureLogConstants.MeterLogs.EVIDENCE_CLEANUP_FAILED,
                        uploadScope.PropertyPublicId);
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    InfrastructureLogConstants.MeterLogs.EVIDENCE_CLEANUP_FAILED,
                    uploadScope.PropertyPublicId);
            }
        }
        catch (Exception exception)
        {
            if (!committed && uploads.Count > 0)
            {
                // Compensate with an independent bounded token so request cancellation cannot abandon newly uploaded objects.
                using var rollbackCleanupCts = new CancellationTokenSource(OBJECT_STORAGE_CLEANUP_TIMEOUT);

                try
                {
                    var cleanup = await ObjectStorageHelper.CleanupUploadedObjectsAsync(
                        _objectStorageService,
                        uploads.Select(upload => upload.Upload),
                        rollbackCleanupCts.Token);

                    if (cleanup.HasFailures)
                    {
                        _logger.LogWarning(
                            exception,
                            InfrastructureLogConstants.MeterLogs.EVIDENCE_CLEANUP_FAILED,
                            uploadScope.PropertyPublicId);
                    }
                }
                catch (Exception cleanupException)
                {
                    // Preserve the original mutation failure while recording that storage compensation also failed.
                    _logger.LogWarning(
                        cleanupException,
                        InfrastructureLogConstants.MeterLogs.EVIDENCE_CLEANUP_FAILED,
                        uploadScope.PropertyPublicId);
                }
            }

            throw;
        }

        // Reload after commit; a response-read failure must never remove already persisted evidence.
        return await GetPeriodAsync(
            new MeterPeriodRequestModel
            {
                RoomId = request.RoomId,
                Month = billingDate.Month,
                Year = billingDate.Year,
                CurrentParty = request.CurrentParty
            },
            cancellationToken);
    }

    /// <summary>
    /// Enforces whether the locked room period must already exist for the requested mutation mode.
    /// </summary>
    /// <param name="mode">The create or update behavior requested by the Application handler.</param>
    /// <param name="hasExistingPeriod">Whether the repository found tracked meter rows for the locked period.</param>
    private static void GuardMutationMode(
        MeterMutationModeEnum mode,
        bool hasExistingPeriod)
    {
        if (mode == MeterMutationModeEnum.Update && !hasExistingPeriod)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        if (mode == MeterMutationModeEnum.Create && hasExistingPeriod)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_ALREADY_EXISTS,
                BAD_REQUEST);
        }
    }

    /// <summary>
    /// Uploads submitted electricity and water evidence as one owner-scoped storage batch.
    /// </summary>
    /// <param name="ownerPublicId">The room public identifier used to isolate object keys.</param>
    /// <param name="electricImages">The optional electricity evidence images in display order.</param>
    /// <param name="waterImages">The optional water evidence images in display order.</param>
    /// <param name="cancellationToken">The token used to cancel the upload.</param>
    /// <returns>The uploaded objects with utility and original-file metadata.</returns>
    private async Task<IReadOnlyList<MeterEvidenceUploadModel>> UploadEvidenceAsync(
        Guid ownerPublicId,
        IReadOnlyList<IFormFile> electricImages,
        IReadOnlyList<IFormFile> waterImages,
        CancellationToken cancellationToken)
    {
        var files = electricImages
            .Select((file, index) => (File: file, Type: MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC, Index: index))
            .Concat(waterImages.Select((file, index) => (File: file, Type: MASTER_CODE_INVOICE_LINE_TYPE_WATER, Index: index)))
            .Select(item => new
            {
                item.File,
                item.Type,
                item.Index,
                SlotName = string.Format(
                    CultureInfo.InvariantCulture,
                    METER_EVIDENCE_SLOT_FORMAT,
                    item.Type.ToLowerInvariant(),
                    item.Index + 1)
            })
            .ToList();

        if (files.Count == 0)
        {
            return [];
        }

        // Unique utility slots preserve submitted order while the shared helper generates collision-safe object keys.
        var uploadRequest = new ObjectStorageUploadBatchRequestModel
        {
            ConfiguredPrefix = _r2StorageOptions.MeterObjectPrefix,
            DefaultPrefix = DEFAULT_METER_OBJECT_PREFIX,
            OwnerPublicId = ownerPublicId,
            Files = files.Select(item => new ObjectStorageUploadFileModel
            {
                SlotName = item.SlotName,
                ObjectTag = item.Type.ToLowerInvariant(),
                File = item.File
            }).ToList()
        };

        // The shared storage helper rolls back objects uploaded earlier in the same batch when a later upload fails.
        var batch = await ObjectStorageHelper.UploadOwnerScopedFormFilesAsync(
            _objectStorageService,
            uploadRequest,
            cancellationToken);

        // Reattach utility metadata so persistence does not need to parse storage keys or infer image order.
        return files.Select(item =>
        {
            var upload = batch.GetUpload(item.SlotName);

            return new MeterEvidenceUploadModel
            {
                File = item.File,
                Upload = upload,
                PublicUrl = ObjectStorageHelper.BuildObjectUrl(
                    _r2StorageOptions.PublicBaseUrl,
                    upload.ObjectKey),
                UtilityTypeCode = item.Type,
                SubmittedOrder = item.Index
            };
        }).ToList();
    }

    /// <summary>
    /// Applies one effective electricity or water policy to its tracked monthly reading.
    /// </summary>
    /// <param name="lineTypeCode">The electricity or water master-data code.</param>
    /// <param name="utilityState">The prepared policy, baseline, and tracked row for this utility.</param>
    /// <param name="context">The locked monthly mutation context.</param>
    /// <returns>A task that completes after the meter record has been created or updated.</returns>
    private async Task ApplyUtilityAsync(
        string lineTypeCode,
        MeterUtilityMutationStateModel utilityState,
        MeterMutationContextModel context)
    {
        var lineType = context.Lookups.GetLineType(lineTypeCode);
        var policy = utilityState.EffectivePolicy;

        var isElectric = lineTypeCode == MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC;
        var current = isElectric ? context.Request.ElectricCurrent : context.Request.WaterCurrent;
        var manualPrevious = isElectric ? context.Request.ElectricPrevious : context.Request.WaterPrevious;
        var submittedPrice = isElectric ? context.Request.ElectricPrice : context.Request.WaterPrice;

        if (policy is null)
        {
            if (current.HasValue || manualPrevious.HasValue || submittedPrice.HasValue)
            {
                throw new ApiException(
                    ApplicationErrorConstants.MeterErrors.ERROR_METER_POLICY_REQUIRED,
                    BAD_REQUEST);
            }

            return;
        }

        if (!current.HasValue)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_SECTION_REQUIRED,
                BAD_REQUEST);
        }

        // The prepared confirmed value is authoritative; only the first recorded period accepts a manual baseline.
        var confirmedStatus = context.Lookups.ConfirmedMeterStatus;
        var previous = utilityState.PreviousConfirmedValue ?? manualPrevious;

        if (!previous.HasValue)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_PREVIOUS_MISMATCH,
                BAD_REQUEST);
        }

        if (current.Value < previous.Value)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_RANGE,
                BAD_REQUEST);
        }

        var reading = utilityState.ExistingMeter;
        if (reading is null)
        {
            reading = new Meter
            {
                PublicId = Guid.NewGuid(),
                PropertyId = context.State.Scope.PropertyId,
                UnitId = context.State.Scope.UnitId,
                LineTypeId = lineType.Id
            };

            await _meterRepository.AddAsync(reading, context.CancellationToken);
            utilityState.ExistingMeter = reading;
        }

        // Persist the server-derived baseline, consumption, price snapshot, and confirmed lifecycle state together.
        reading.ChargePolicyId = policy.Id;
        reading.BillingPeriodFrom = context.Periods.CurrentFrom;
        reading.BillingPeriodTo = context.Periods.CurrentTo;
        reading.ReadingDate = context.BillingDate;
        reading.PreviousReading = previous;
        reading.CurrentReading = current;
        reading.UsageQuantity = current - previous;
        reading.UnitPriceSnapshot = submittedPrice ?? policy.Amount;
        reading.StatusId = confirmedStatus.Id;
        reading.SubmittedByPartyId = context.Request.CurrentParty.PartyId;
        reading.SubmittedAt = DateTime.UtcNow;
        reading.ReviewedByPartyId = context.Request.CurrentParty.PartyId;
        reading.ReviewedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Synchronises confirmed readings with the following month's invoice.
    /// </summary>
    /// <param name="scope">The landlord-scoped room and property identifiers.</param>
    /// <param name="periods">The current meter period and adjacent invoice period boundaries.</param>
    /// <param name="lookups">The typed master-data values required to rebuild invoice lines.</param>
    /// <param name="meters">The tracked electricity and water rows already saved in this transaction.</param>
    /// <param name="cancellationToken">The token used to cancel invoice synchronisation.</param>
    /// <returns>A task that completes after the affected invoice has been updated or replaced.</returns>
    private async Task SynchronizeInvoiceAsync(
        MeterScopeModel scope,
        MeterPeriodsModel periods,
        MeterMutationLookupModel lookups,
        IReadOnlyCollection<Meter> meters,
        CancellationToken cancellationToken)
    {
        var nextFrom = periods.CurrentFrom.AddMonths(1);
        var nextTo = nextFrom.AddMonths(1).AddDays(-1);
        var invoice = await _invoiceUtilityRepository.GetInvoiceGraphAsync(
            scope.UnitId,
            nextFrom,
            nextTo,
            cancellationToken);

        if (invoice is null)
        {
            return;
        }

        // Payment state is rechecked on the locked invoice before any utility line or issued invoice is changed.
        var hasPayment = invoice.PaidAmount > 0
                         || await _invoiceUtilityRepository.HasSuccessfulPaymentAsync(invoice.Id, cancellationToken);
        if (hasPayment
            || invoice.StatusId == lookups.PartiallyPaidInvoiceStatus.Id
            || invoice.StatusId == lookups.PaidInvoiceStatus.Id)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_MUTATION_BLOCKED,
                BAD_REQUEST);
        }

        var chargeModeId = lookups.MeterReadingChargeMode.Id;

        if (invoice.StatusId == lookups.DraftInvoiceStatus.Id)
        {
            await _invoiceUtilityRepository.ReplaceUtilityLinesAsync(
                invoice,
                meters,
                chargeModeId,
                cancellationToken);
            return;
        }

        if (invoice.StatusId == lookups.IssuedInvoiceStatus.Id
            || invoice.StatusId == lookups.OverdueInvoiceStatus.Id)
        {
            // The mutation itself is final intent, so an unpaid issued invoice is replaced without an echoed UI flag.
            invoice.StatusId = lookups.CancelledInvoiceStatus.Id;
            await _invoiceUtilityRepository.CreateReplacementAsync(
                invoice,
                lookups.DraftInvoiceStatus.Id,
                meters,
                chargeModeId,
                cancellationToken);
        }
    }

    /// <summary>
    /// Persists newly uploaded evidence against the matching electricity or water meter record.
    /// </summary>
    /// <param name="uploads">The uploaded evidence grouped by electricity or water slot.</param>
    /// <param name="deletedElectricImageIds">The electricity evidence identifiers explicitly selected for removal.</param>
    /// <param name="deletedWaterImageIds">The water evidence identifiers explicitly selected for removal.</param>
    /// <param name="context">The active meter mutation context.</param>
    /// <param name="replacedObjectKeys">The old storage objects scheduled for post-commit clean-up.</param>
    /// <param name="cancellationToken">The token used to cancel persistence work.</param>
    /// <returns>A task that completes after document metadata and links have been staged.</returns>
    private async Task PersistEvidenceAsync(
        IReadOnlyCollection<MeterEvidenceUploadModel> uploads,
        IReadOnlyCollection<Guid> deletedElectricImageIds,
        IReadOnlyCollection<Guid> deletedWaterImageIds,
        MeterMutationContextModel context,
        ICollection<string> replacedObjectKeys,
        CancellationToken cancellationToken)
    {
        if (uploads.Count == 0
            && deletedElectricImageIds.Count == 0
            && deletedWaterImageIds.Count == 0)
        {
            return;
        }

        // The tracked meter rows already carry internal IDs, so evidence can attach without reloading the locked period.
        var meters = context.State.GetMeters();
        var evidenceLookups = context.Lookups.Evidence;
        var electricMeter = meters.FirstOrDefault(meter => meter.LineTypeId == context.Lookups.ElectricLineType.Id);
        var waterMeter = meters.FirstOrDefault(meter => meter.LineTypeId == context.Lookups.WaterLineType.Id);
        var electricUploads = uploads
            .Where(upload => upload.UtilityTypeCode == MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC)
            .OrderBy(upload => upload.SubmittedOrder)
            .ToList();
        var waterUploads = uploads
            .Where(upload => upload.UtilityTypeCode == MASTER_CODE_INVOICE_LINE_TYPE_WATER)
            .OrderBy(upload => upload.SubmittedOrder)
            .ToList();

        var hasElectricEvidenceChanges = electricUploads.Count > 0 || deletedElectricImageIds.Count > 0;
        var hasWaterEvidenceChanges = waterUploads.Count > 0 || deletedWaterImageIds.Count > 0;

        // Images can belong only to a utility that is active for this room's confirmed meter period.
        if (hasElectricEvidenceChanges && electricMeter is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_POLICY_REQUIRED,
                BAD_REQUEST);
        }

        if (hasWaterEvidenceChanges && waterMeter is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_POLICY_REQUIRED,
                BAD_REQUEST);
        }

        // Load all active room-period evidence once so additions and explicitly selected removals share one authoritative view.
        var oldLinks = await _meterRepository.GetEvidenceLinksForMutationAsync(
            meters.Select(meter => meter.Id).ToArray(),
            evidenceLookups.MeterEntityType.Id,
            evidenceLookups.MeterPhotoLinkType.Id,
            cancellationToken);
        var electricDeletedIds = deletedElectricImageIds.ToHashSet();
        var waterDeletedIds = deletedWaterImageIds.ToHashSet();
        var deletedIds = electricDeletedIds
            .Concat(waterDeletedIds)
            .ToHashSet();
        var selectedLinks = oldLinks
            .Where(link => deletedIds.Contains(link.PublicId))
            .ToList();

        // The selected IDs must belong to the intended utility meter in this room-period; no foreign image can be removed.
        if (selectedLinks.Count != deletedIds.Count
            || selectedLinks.Any(link => electricDeletedIds.Contains(link.PublicId) && link.EntityId != electricMeter?.Id)
            || selectedLinks.Any(link => waterDeletedIds.Contains(link.PublicId) && link.EntityId != waterMeter?.Id))
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_NOT_FOUND,
                NOT_FOUND,
                StatusCodes.Status404NotFound);
        }

        // Retire only the images selected by the user and defer physical object removal until this transaction commits.
        foreach (var oldLink in selectedLinks)
        {
            oldLink.IsDeleted = true;
            oldLink.Document.IsDeleted = true;
            replacedObjectKeys.Add(oldLink.Document.StoragePath);
        }

        var activeLinks = oldLinks.Except(selectedLinks).ToList();
        var documents = new List<Document>();
        var links = new List<DocumentLink>();

        // Append submitted electricity and water images independently while preserving each utility's active display order.
        AddEvidenceForMeter(
            electricMeter,
            electricUploads,
            activeLinks,
            evidenceLookups,
            documents,
            links);
        AddEvidenceForMeter(
            waterMeter,
            waterUploads,
            activeLinks,
            evidenceLookups,
            documents,
            links);

        // Stage document metadata and links with the meter transaction so failed mutations do not leave DB metadata behind.
        await _meterRepository.AddEvidenceAsync(documents, links, cancellationToken);
    }

    /// <summary>
    /// Appends one utility's validated images while preserving the surviving active display order.
    /// </summary>
    /// <param name="meter">The persisted electricity or water meter receiving the images.</param>
    /// <param name="uploads">The new images submitted for that utility in display order.</param>
    /// <param name="activeLinks">The active links that remain after selected deletion.</param>
    /// <param name="lookups">The resolved document and link persistence values.</param>
    /// <param name="documents">The document collection staged by the surrounding transaction.</param>
    /// <param name="links">The link collection staged by the surrounding transaction.</param>
    private static void AddEvidenceForMeter(
        Meter meter,
        IReadOnlyCollection<MeterEvidenceUploadModel> uploads,
        IReadOnlyCollection<DocumentLink> activeLinks,
        MeterEvidenceLookupModel lookups,
        ICollection<Document> documents,
        ICollection<DocumentLink> links)
    {
        if (uploads.Count == 0)
        {
            return;
        }

        var activeImageCount = activeLinks.Count(link => link.EntityId == meter.Id);
        if (activeImageCount + uploads.Count > MAX_METER_EVIDENCE_IMAGE_COUNT)
        {
            throw new ApiException(
                ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_LIMIT,
                BAD_REQUEST);
        }

        var nextSortOrder = activeLinks
            .Where(link => link.EntityId == meter.Id)
            .Select(link => link.SortOrder)
            .DefaultIfEmpty(-1)
            .Max() + 1;

        // Every validated upload creates independent document metadata and an ordered meter-photo link.
        foreach (var upload in uploads)
        {
            var document = CreateEvidenceDocument(upload, lookups);
            documents.Add(document);
            links.Add(CreateEvidenceLink(document, meter.Id, nextSortOrder, lookups));
            nextSortOrder++;
        }
    }

    /// <summary>
    /// Creates persisted document metadata for one uploaded meter image.
    /// </summary>
    /// <param name="upload">The validated storage upload and original form file.</param>
    /// <param name="lookups">The resolved document master-data values.</param>
    /// <returns>The document entity linked to the uploaded object.</returns>
    private static Document CreateEvidenceDocument(
        MeterEvidenceUploadModel upload,
        MeterEvidenceLookupModel lookups)
    {
        return new Document
        {
            PublicId = Guid.NewGuid(),
            DocumentTypeId = lookups.DocumentType.Id,
            FileName = Path.GetFileName(upload.Upload.ObjectKey),
            OriginalFileName = upload.File.FileName,
            ContentType = upload.Upload.ContentType,
            FileExtension = Path.GetExtension(upload.Upload.ObjectKey),
            FileSize = upload.Upload.FileSize,
            StorageProviderId = lookups.StorageProvider.Id,
            StoragePath = upload.Upload.ObjectKey,
            FileUrl = upload.PublicUrl,
            Checksum = upload.Upload.Checksum,
            StatusId = lookups.ActiveDocumentStatus.Id
        };
    }

    /// <summary>
    /// Creates the typed document link for one meter record.
    /// </summary>
    /// <param name="document">The document entity being linked.</param>
    /// <param name="meterId">The internal meter record identifier.</param>
    /// <param name="sortOrder">The display order after surviving meter images.</param>
    /// <param name="lookups">The resolved entity, link, and status master-data values.</param>
    /// <returns>The document link for the meter evidence.</returns>
    private static DocumentLink CreateEvidenceLink(
        Document document,
        long meterId,
        int sortOrder,
        MeterEvidenceLookupModel lookups)
    {
        return new DocumentLink
        {
            PublicId = Guid.NewGuid(),
            Document = document,
            EntityTypeId = lookups.MeterEntityType.Id,
            EntityId = meterId,
            LinkTypeId = lookups.MeterPhotoLinkType.Id,
            IsPrimary = false,
            SortOrder = sortOrder,
            StatusId = lookups.ActiveLinkStatus.Id
        };
    }

    /// <summary>
    /// Loads the room inside the current active landlord relationship scope.
    /// </summary>
    /// <param name="roomId">The frontend-safe room identifier.</param>
    /// <param name="currentPartyId">The internal current landlord party identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the scoped query.</param>
    /// <returns>The scoped room and property identifiers.</returns>
    private async Task<MeterScopeModel> GetScopeAsync(
        Guid roomId,
        long currentPartyId,
        CancellationToken cancellationToken)
    {
        return await _meterRepository.GetScopeAsync(
                   roomId,
                   currentPartyId,
                   PROPERTY_ACCESS_RELATIONSHIP_CODES,
                   cancellationToken)
               ?? throw new ApiException(
                   ApplicationErrorConstants.RoomErrors.ERROR_ROOM_NOT_FOUND,
                   NOT_FOUND,
                   StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Loads evidence rows keyed by their internal meter record identifier.
    /// </summary>
    /// <param name="rows">The meter rows whose evidence should be loaded.</param>
    /// <param name="cancellationToken">The token used to cancel the evidence query.</param>
    /// <returns>The active evidence rows for each meter record in display order.</returns>
    private async Task<IReadOnlyDictionary<long, IReadOnlyList<MeterEvidenceRowModel>>> LoadEvidenceAsync(
        IReadOnlyCollection<MeterRowModel> rows,
        CancellationToken cancellationToken)
    {
        var meterIds = rows
            .Where(row => row.MeterId.HasValue)
            .Select(row => row.MeterId!.Value)
            .Distinct()
            .ToArray();

        var evidence = await _meterRepository.GetEvidenceAsync(meterIds, cancellationToken);

        return evidence
            .GroupBy(item => item.MeterId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<MeterEvidenceRowModel>)group
                    .OrderBy(item => item.SortOrder)
                    .ToList());
    }

    /// <summary>
    /// Returns the invoice action currently visible to the meter screen.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="periods">The meter period used to locate the following month's invoice.</param>
    /// <param name="cancellationToken">The token used to cancel the invoice query.</param>
    /// <returns>The current save action and blocking reason.</returns>
    private async Task<MeterInvoiceImpactResponseDto> GetInvoiceImpactAsync(
        long unitId,
        MeterPeriodsModel periods,
        CancellationToken cancellationToken)
    {
        var nextFrom = periods.CurrentFrom.AddMonths(1);
        var nextTo = nextFrom.AddMonths(1).AddDays(-1);
        var invoice = await _invoiceUtilityRepository.GetInvoiceImpactAsync(
            unitId,
            nextFrom,
            nextTo,
            cancellationToken);

        if (invoice is null)
        {
            return new MeterInvoiceImpactResponseDto
            {
                ActionCode = UTILITY_INVOICE_ACTION_WAITING,
                CanSave = true
            };
        }

        var blocked = invoice.PaidAmount > 0
                      || invoice.HasSuccessfulPayment
                      || invoice.StatusCode is MASTER_CODE_INVOICE_STATUS_PARTIALLY_PAID or MASTER_CODE_INVOICE_STATUS_PAID;
        string actionCode;
        if (blocked)
        {
            actionCode = UTILITY_INVOICE_ACTION_BLOCKED_PAID;
        }
        else if (invoice.StatusCode is MASTER_CODE_INVOICE_STATUS_ISSUED or MASTER_CODE_INVOICE_STATUS_OVERDUE)
        {
            actionCode = UTILITY_INVOICE_ACTION_REPLACE_ISSUED;
        }
        else
        {
            actionCode = UTILITY_INVOICE_ACTION_UPDATE_DRAFT;
        }

        return new MeterInvoiceImpactResponseDto
        {
            ActionCode = actionCode,
            CanSave = !blocked,
            BlockedReasonCode = blocked
                ? ApplicationErrorConstants.MeterErrors.ERROR_METER_MUTATION_BLOCKED
                : null,
            Invoice = new MeterInvoiceCompactResponseDto
            {
                Id = invoice.InvoicePublicId,
                StatusCode = invoice.StatusCode
            }
        };
    }

    /// <summary>
    /// Resolves only master-data values required by a confirmed room meter mutation.
    /// </summary>
    /// <param name="includeEvidence">Whether evidence document and link values are required.</param>
    /// <param name="cancellationToken">The token used to cancel master-data resolution.</param>
    /// <returns>The typed master-data values required by the mutation.</returns>
    private async Task<MeterMutationLookupModel> ResolveMutationLookupsAsync(
        bool includeEvidence,
        CancellationToken cancellationToken)
    {
        // The typed lookup model owns its exact key contract; the service performs one bounded master-data read.
        var keys = MeterMutationLookupModel.GetRequiredKeys(includeEvidence);
        var values = await _masterDataService.GetValuesAsync(keys, cancellationToken);

        // Convert the transient dictionary once so mutation phases consume named values instead of repeated lookups.
        return MeterMutationLookupModel.Create(values, includeEvidence);
    }

}
