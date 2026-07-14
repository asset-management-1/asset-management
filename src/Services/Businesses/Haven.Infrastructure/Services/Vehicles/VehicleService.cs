namespace Haven.Infrastructure.Services.Vehicles;

/// <summary>
/// Provides vehicle registration workflows for landlord-managed rooms.
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMasterDataService _masterDataService;
    private readonly IObjectStorageService _objectStorageService;
    private readonly IImageOptimizationService _imageOptimizationService;
    private readonly R2StorageOptions _r2StorageOptions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VehicleService> _logger;

    /// <summary>
    /// Creates the vehicle service with its persistence, lookup, storage, transaction, and logging dependencies.
    /// </summary>
    /// <param name="vehicleRepository">The repository used for vehicle reads and tracked mutations.</param>
    /// <param name="masterDataService">The service used to resolve submitted master-data codes.</param>
    /// <param name="objectStorageService">The service used to upload and delete vehicle image objects.</param>
    /// <param name="imageOptimizationService">The service used to decode and validate submitted image content.</param>
    /// <param name="r2StorageOptions">The configured object-storage prefixes and public URL.</param>
    /// <param name="unitOfWork">The transaction boundary for vehicle mutations.</param>
    /// <param name="logger">The structured vehicle workflow logger.</param>
    public VehicleService(
        IVehicleRepository vehicleRepository,
        IMasterDataService masterDataService,
        IObjectStorageService objectStorageService,
        IImageOptimizationService imageOptimizationService,
        IOptions<R2StorageOptions> r2StorageOptions,
        IUnitOfWork unitOfWork,
        ILogger<VehicleService> logger)
    {
        _vehicleRepository = vehicleRepository;
        _masterDataService = masterDataService;
        _objectStorageService = objectStorageService;
        _imageOptimizationService = imageOptimizationService;
        _r2StorageOptions = r2StorageOptions.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Loads every active vehicle for a landlord-managed room.
    /// </summary>
    /// <param name="request">The landlord and room scope.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The compact room vehicle collection.</returns>
    public async Task<IReadOnlyList<VehicleListItemResponseDto>> GetVehiclesAsync(
        VehicleListRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Map the required landlord and room scope into the bounded repository read contract.
        var parameters = request.Adapt<VehicleListQueryParametersModel>();
        var rows = await _vehicleRepository.GetByRoomAsync(parameters, cancellationToken);

        // Flat rows map directly to the compact management response and intentionally exclude image data.
        var response = rows.Adapt<List<VehicleListItemResponseDto>>();

        _logger.LogInformation(
            InfrastructureLogConstants.VehicleLogs.VEHICLES_LOADED,
            rows.Count,
            request.CurrentParty.PartyPublicId);

        return response;
    }

    /// <summary>
    /// Creates one vehicle for an eligible primary tenant payer.
    /// </summary>
    /// <param name="request">The validated vehicle creation request and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the workflow.</param>
    /// <returns>The persisted vehicle detail.</returns>
    public async Task<VehicleDetailResponseDto> CreateAsync(
        VehicleCreateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Resolve only the master-data values required to persist the selected vehicle type and payer status checks.
        var values = await _masterDataService.GetValuesAsync(
            [
                new(MasterDataTypeEnum.VehicleType, request.VehicleTypeCode),
                new(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_ACTIVE),
                new(MasterDataTypeEnum.ContractStatus, MASTER_CODE_CONTRACT_STATUS_ACTIVE)
            ],
            cancellationToken);
        var vehicleType = values.GetValue(MasterDataTypeEnum.VehicleType, request.VehicleTypeCode);
        var activeOccupancy = values.GetValue(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_ACTIVE);
        var activeContract = values.GetValue(MasterDataTypeEnum.ContractStatus, MASTER_CODE_CONTRACT_STATUS_ACTIVE);

        // The selected payer must sign an active contract for the selected room.
        var payer = await _vehicleRepository.GetPrimaryPayerAsync(
            new VehiclePayerEligibilityQueryParametersModel
            {
                RoomPublicId = request.RoomId,
                PayerTenantPublicId = request.PayerTenantId,
                CurrentPartyId = request.CurrentParty.PartyId,
                ActiveOccupancyStatusId = activeOccupancy.Id,
                ActiveContractStatusId = activeContract.Id,
                RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
            },
            cancellationToken);
        if (payer is null)
        {
            throw new ApiException(
                ApplicationErrorConstants.VehicleErrors.ERROR_VEHICLE_PRIMARY_PAYER_REQUIRED,
                BAD_REQUEST);
        }

        // Map submitted vehicle text, then apply database-owned relationships resolved above.
        var vehicle = request.Adapt<PartyVehicle>();
        vehicle.PublicId = Guid.NewGuid();
        vehicle.UnitId = payer.UnitId;
        vehicle.PartyId = payer.PartyId;
        vehicle.VehicleTypeId = vehicleType.Id;
        // Validate and upload every optional image slot submitted for the new vehicle before persistence starts.
        var uploads = await UploadVehicleImagesAsync(
            vehicle.PublicId,
            request.RegistrationFrontImage,
            request.RegistrationSideImage,
            request.VehicleFrontImage,
            request.VehicleSideImage,
            cancellationToken);

        // Attach uploaded object URLs before the new vehicle record is committed.
        ApplyUploadedImageUrls(vehicle, uploads);

        try
        {
            // Persist only after every optional upload is successful, so the row never points at failed objects.
            await _unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                await _vehicleRepository.AddAsync(vehicle, transactionToken);
            }, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Database persistence failed after upload, so remove only objects created by this request.
            await CleanupUploadsAsync(vehicle.PublicId, uploads, cancellationToken);
            throw;
        }

        _logger.LogInformation(
            InfrastructureLogConstants.VehicleLogs.VEHICLE_CREATED,
            vehicle.PublicId,
            request.CurrentParty.PartyPublicId);

        return await GetDetailAsync(
            new VehicleDetailRequestModel
            {
                RoomPublicId = request.RoomId,
                VehiclePublicId = vehicle.PublicId,
                CurrentParty = request.CurrentParty
            },
            cancellationToken);
    }

    /// <summary>
    /// Loads one vehicle registration detail within the current landlord scope.
    /// </summary>
    /// <param name="request">The vehicle identifier and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the read.</param>
    /// <returns>The scoped vehicle detail.</returns>
    public async Task<VehicleDetailResponseDto> GetDetailAsync(
        VehicleDetailRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Read the exact display projection and keep unauthorized resources indistinguishable from missing ones.
        var row = await _vehicleRepository.GetDetailAsync(
                      new VehicleScopeQueryParametersModel
                      {
                          RoomPublicId = request.RoomPublicId,
                          VehiclePublicId = request.VehiclePublicId,
                          CurrentPartyId = request.CurrentParty.PartyId,
                          RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
                      },
                      cancellationToken)
                  ?? throw new ApiException(
                      ApplicationErrorConstants.VehicleErrors.ERROR_VEHICLE_NOT_FOUND,
                      NOT_FOUND,
                      StatusCodes.Status404NotFound);

        return row.Adapt<VehicleDetailResponseDto>();
    }

    /// <summary>
    /// Updates submitted vehicle fields without changing the room attachment.
    /// </summary>
    /// <param name="request">The validated partial vehicle update and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the workflow.</param>
    /// <returns>The refreshed vehicle detail.</returns>
    public async Task<VehicleDetailResponseDto> UpdateAsync(
        VehicleUpdateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Load a tracked vehicle inside the current landlord scope before resolving any submitted relationship changes.
        var vehicle = await _vehicleRepository.GetForMutationAsync(
                          new VehicleScopeQueryParametersModel
                          {
                              RoomPublicId = request.RoomId,
                              VehiclePublicId = request.VehicleId,
                              CurrentPartyId = request.CurrentParty.PartyId,
                              RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
                          },
                          cancellationToken)
                      ?? throw new ApiException(
                          ApplicationErrorConstants.VehicleErrors.ERROR_VEHICLE_NOT_FOUND,
                          NOT_FOUND,
                          StatusCodes.Status404NotFound);

        // Resolve active occupancy/contract gates and the optional vehicle type before validating a payer reassignment.
        var masterDataKeys = new List<MasterDataKeyModel>
        {
            new(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_ACTIVE),
            new(MasterDataTypeEnum.ContractStatus, MASTER_CODE_CONTRACT_STATUS_ACTIVE)
        };
        if (request.VehicleTypeCode is not null)
        {
            masterDataKeys.Add(new MasterDataKeyModel(MasterDataTypeEnum.VehicleType, request.VehicleTypeCode));
        }

        var values = await _masterDataService.GetValuesAsync(masterDataKeys, cancellationToken);
        var activeOccupancy = values.GetValue(MasterDataTypeEnum.OccupancyStatus, MASTER_CODE_OCCUPANCY_STATUS_ACTIVE);
        var activeContract = values.GetValue(MasterDataTypeEnum.ContractStatus, MASTER_CODE_CONTRACT_STATUS_ACTIVE);

        if (request.PayerTenantId.HasValue)
        {
            // Payer reassignment stays inside the tracked room and avoids an unnecessary display-detail read.
            var payer = await _vehicleRepository.GetPrimaryPayerAsync(
                new VehiclePayerEligibilityQueryParametersModel
                {
                    UnitId = vehicle.UnitId,
                    PayerTenantPublicId = request.PayerTenantId.Value,
                    CurrentPartyId = request.CurrentParty.PartyId,
                    ActiveOccupancyStatusId = activeOccupancy.Id,
                    ActiveContractStatusId = activeContract.Id,
                    RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
                },
                cancellationToken);

            if (payer is null)
            {
                throw new ApiException(
                    ApplicationErrorConstants.VehicleErrors.ERROR_VEHICLE_PRIMARY_PAYER_REQUIRED,
                    BAD_REQUEST);
            }

            vehicle.PartyId = payer.PartyId;
        }

        // Map only submitted flat scalar fields and apply the optional vehicle-type lookup explicitly.
        request.Adapt(vehicle);
        if (request.VehicleTypeCode is not null)
        {
            vehicle.VehicleTypeId = values.GetValue(MasterDataTypeEnum.VehicleType, request.VehicleTypeCode).Id;
        }

        // Capture current URLs before replacement so only successfully superseded objects are removed after commit.
        var replacedImageUrls = new[]
        {
            vehicle.RegistrationFrontImageUrl,
            vehicle.RegistrationSideImageUrl,
            vehicle.VehicleFrontImageUrl,
            vehicle.VehicleSideImageUrl
        };

        // Validate and upload only replacement slots; omitted slots retain the URLs captured above.
        var uploads = await UploadVehicleImagesAsync(
            vehicle.PublicId,
            request.RegistrationFrontImage,
            request.RegistrationSideImage,
            request.VehicleFrontImage,
            request.VehicleSideImage,
            cancellationToken);

        // Apply replacement URLs to the tracked entity before its database update is committed.
        ApplyUploadedImageUrls(vehicle, uploads);

        try
        {
            // The tracked vehicle is saved in one transaction after every submitted image has uploaded successfully.
            await _unitOfWork.ExecuteInTransactionAsync(async _ =>
            {
                await _vehicleRepository.UpdateAsync(vehicle);
            }, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Remove only objects created by this failed update; existing image objects remain untouched.
            await CleanupUploadsAsync(vehicle.PublicId, uploads, cancellationToken);
            throw;
        }

        // Remove old objects only after the replacement URLs are durable.
        // Cleanup failures do not roll back an otherwise valid vehicle update.
        await CleanupReplacedImageObjectsAsync(
            vehicle.PublicId,
            replacedImageUrls,
            uploads,
            cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.VehicleLogs.VEHICLE_UPDATED,
            vehicle.PublicId,
            request.CurrentParty.PartyPublicId);

        return await GetDetailAsync(
            new VehicleDetailRequestModel
            {
                RoomPublicId = request.RoomId,
                VehiclePublicId = vehicle.PublicId,
                CurrentParty = request.CurrentParty
            },
            cancellationToken);
    }

    /// <summary>
    /// Soft-deletes one vehicle while preserving billing references and uploaded-object history.
    /// </summary>
    /// <param name="request">The vehicle identifier and current landlord context.</param>
    /// <param name="cancellationToken">The token used to cancel the mutation.</param>
    /// <returns>The successful operation status.</returns>
    public async Task<OperationStatusResponseDto> DeleteAsync(
        VehicleDeleteRequestModel request,
        CancellationToken cancellationToken = default)
    {
        // Scope the tracked record before deletion so unavailable vehicles resolve as not found.
        var vehicle = await _vehicleRepository.GetForMutationAsync(
                          new VehicleScopeQueryParametersModel
                          {
                              RoomPublicId = request.RoomPublicId,
                              VehiclePublicId = request.VehiclePublicId,
                              CurrentPartyId = request.CurrentParty.PartyId,
                              RelationshipCodes = PROPERTY_ACCESS_RELATIONSHIP_CODES
                          },
                          cancellationToken)
                      ?? throw new ApiException(
                          ApplicationErrorConstants.VehicleErrors.ERROR_VEHICLE_NOT_FOUND,
                          NOT_FOUND,
                          StatusCodes.Status404NotFound);

        // Soft-delete the tracked row in one transaction while preserving historical references and image objects.
        await _unitOfWork.ExecuteInTransactionAsync(async _ =>
        {
            vehicle.IsDeleted = true;
            await _vehicleRepository.UpdateAsync(vehicle);
        }, cancellationToken);

        _logger.LogInformation(
            InfrastructureLogConstants.VehicleLogs.VEHICLE_DELETED,
            vehicle.PublicId,
            request.CurrentParty.PartyPublicId);

        return OperationStatusResponseHelper.Success(
            ApplicationMessageConstants.VehicleMessages.VEHICLE_DELETE_SUCCESS_MESSAGE);
    }

    /// <summary>
    /// Decodes and uploads submitted vehicle image slots as one rollback-safe batch without transforming their bytes.
    /// </summary>
    /// <param name="vehiclePublicId">The vehicle owner identifier used in object keys.</param>
    /// <param name="registrationFrontImage">The optional registration front image.</param>
    /// <param name="registrationSideImage">The optional registration side image.</param>
    /// <param name="vehicleFrontImage">The optional vehicle front image.</param>
    /// <param name="vehicleSideImage">The optional vehicle side image.</param>
    /// <param name="cancellationToken">The token used to cancel validation and upload work.</param>
    /// <returns>The uploaded objects keyed by vehicle image slot.</returns>
    private async Task<ObjectStorageUploadBatchResultModel> UploadVehicleImagesAsync(
        Guid vehiclePublicId,
        IFormFile registrationFrontImage,
        IFormFile registrationSideImage,
        IFormFile vehicleFrontImage,
        IFormFile vehicleSideImage,
        CancellationToken cancellationToken)
    {
        try
        {
            // Decode all supplied images before the shared uploader persists their original bytes.
            return await ObjectStorageHelper.UploadOwnerScopedValidatedImageFormFilesAsync(
                _objectStorageService,
                _imageOptimizationService,
                new ObjectStorageUploadBatchRequestModel
                {
                    ConfiguredPrefix = _r2StorageOptions.VehicleObjectPrefix,
                    DefaultPrefix = DEFAULT_VEHICLE_OBJECT_PREFIX,
                    OwnerPublicId = vehiclePublicId,
                    Files =
                    [
                        new()
                        {
                            SlotName = VEHICLE_REGISTRATION_FRONT_IMAGE_SLOT,
                            ObjectTag = VEHICLE_REGISTRATION_FRONT_IMAGE_SLOT,
                            File = registrationFrontImage
                        },
                        new()
                        {
                            SlotName = VEHICLE_REGISTRATION_SIDE_IMAGE_SLOT,
                            ObjectTag = VEHICLE_REGISTRATION_SIDE_IMAGE_SLOT,
                            File = registrationSideImage
                        },
                        new()
                        {
                            SlotName = VEHICLE_FRONT_IMAGE_SLOT,
                            ObjectTag = VEHICLE_FRONT_IMAGE_SLOT,
                            File = vehicleFrontImage
                        },
                        new()
                        {
                            SlotName = VEHICLE_SIDE_IMAGE_SLOT,
                            ObjectTag = VEHICLE_SIDE_IMAGE_SLOT,
                            File = vehicleSideImage
                        }
                    ]
                },
                cancellationToken);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.VehicleLogs.VEHICLE_IMAGE_UPLOAD_FAILED,
                vehiclePublicId);
            throw new ApiException(
                IMAGE_FILE_INVALID_MESSAGE,
                BAD_REQUEST,
                StatusCodes.Status400BadRequest);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                InfrastructureLogConstants.VehicleLogs.VEHICLE_IMAGE_UPLOAD_FAILED,
                vehiclePublicId);
            throw new ApiException(
                ApplicationErrorConstants.VehicleErrors.ERROR_VEHICLE_IMAGE_UPLOAD_FAILED,
                SERVICE_UNAVAILABLE,
                StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Applies uploaded object URLs while preserving slots omitted from a partial update.
    /// </summary>
    /// <param name="vehicle">The tracked vehicle receiving replacement URLs.</param>
    /// <param name="uploads">The newly uploaded objects keyed by slot.</param>
    private void ApplyUploadedImageUrls(
        PartyVehicle vehicle,
        ObjectStorageUploadBatchResultModel uploads)
    {
        // Only submitted files replace their matching slot; omitted files retain the persisted URL during updates.
        var registrationFront = uploads.GetUpload(VEHICLE_REGISTRATION_FRONT_IMAGE_SLOT);
        var registrationSide = uploads.GetUpload(VEHICLE_REGISTRATION_SIDE_IMAGE_SLOT);
        var vehicleFront = uploads.GetUpload(VEHICLE_FRONT_IMAGE_SLOT);
        var vehicleSide = uploads.GetUpload(VEHICLE_SIDE_IMAGE_SLOT);

        if (registrationFront is not null)
        {
            vehicle.RegistrationFrontImageUrl = ObjectStorageHelper.BuildObjectUrl(
                _r2StorageOptions.PublicBaseUrl,
                registrationFront.ObjectKey);
        }

        if (registrationSide is not null)
        {
            vehicle.RegistrationSideImageUrl = ObjectStorageHelper.BuildObjectUrl(
                _r2StorageOptions.PublicBaseUrl,
                registrationSide.ObjectKey);
        }

        if (vehicleFront is not null)
        {
            vehicle.VehicleFrontImageUrl = ObjectStorageHelper.BuildObjectUrl(
                _r2StorageOptions.PublicBaseUrl,
                vehicleFront.ObjectKey);
        }

        if (vehicleSide is not null)
        {
            vehicle.VehicleSideImageUrl = ObjectStorageHelper.BuildObjectUrl(
                _r2StorageOptions.PublicBaseUrl,
                vehicleSide.ObjectKey);
        }
    }

    /// <summary>
    /// Cleans up objects uploaded by a failed database mutation.
    /// </summary>
    /// <param name="vehiclePublicId">The vehicle identifier used in cleanup logs.</param>
    /// <param name="uploads">The objects created by the failed request.</param>
    /// <param name="cancellationToken">The token used to cancel cleanup.</param>
    private async Task CleanupUploadsAsync(
        Guid vehiclePublicId,
        ObjectStorageUploadBatchResultModel uploads,
        CancellationToken cancellationToken)
    {
        // Cleanup is best-effort because the original persistence error remains the client-visible failure.
        var cleanup = await ObjectStorageHelper.CleanupUploadedObjectsAsync(
            _objectStorageService,
            uploads.UploadsBySlot.Values,
            cancellationToken);

        if (cleanup.HasFailures)
        {
            _logger.LogWarning(
                InfrastructureLogConstants.VehicleLogs.VEHICLE_IMAGE_CLEANUP_FAILED,
                vehiclePublicId);
        }
    }

    /// <summary>
    /// Removes replaced image objects after replacement URLs are committed.
    /// </summary>
    /// <param name="vehiclePublicId">The vehicle identifier used in cleanup logs.</param>
    /// <param name="previousImageUrls">The image URLs captured before mutation.</param>
    /// <param name="uploads">The slots replaced by the current request.</param>
    /// <param name="cancellationToken">The token used to cancel cleanup.</param>
    private async Task CleanupReplacedImageObjectsAsync(
        Guid vehiclePublicId,
        IReadOnlyList<string> previousImageUrls,
        ObjectStorageUploadBatchResultModel uploads,
        CancellationToken cancellationToken)
    {
        // Derive keys only for slots replaced by this request; omitted slots keep their stored objects.
        var previousKeys = new[]
        {
            uploads.GetUpload(VEHICLE_REGISTRATION_FRONT_IMAGE_SLOT) is null
                ? null
                : ObjectStorageHelper.GetObjectKey(_r2StorageOptions.PublicBaseUrl, previousImageUrls[0]),
            uploads.GetUpload(VEHICLE_REGISTRATION_SIDE_IMAGE_SLOT) is null
                ? null
                : ObjectStorageHelper.GetObjectKey(_r2StorageOptions.PublicBaseUrl, previousImageUrls[1]),
            uploads.GetUpload(VEHICLE_FRONT_IMAGE_SLOT) is null
                ? null
                : ObjectStorageHelper.GetObjectKey(_r2StorageOptions.PublicBaseUrl, previousImageUrls[2]),
            uploads.GetUpload(VEHICLE_SIDE_IMAGE_SLOT) is null
                ? null
                : ObjectStorageHelper.GetObjectKey(_r2StorageOptions.PublicBaseUrl, previousImageUrls[3])
        };
        var cleanup = await ObjectStorageHelper.CleanupObjectKeysAsync(
            _objectStorageService,
            previousKeys,
            cancellationToken);

        if (cleanup.HasFailures)
        {
            _logger.LogWarning(
                InfrastructureLogConstants.VehicleLogs.VEHICLE_IMAGE_CLEANUP_FAILED,
                vehiclePublicId);
        }
    }
}
