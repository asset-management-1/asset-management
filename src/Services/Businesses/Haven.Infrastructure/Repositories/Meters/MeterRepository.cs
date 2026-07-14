namespace Haven.Infrastructure.Repositories.Meters;

/// <summary>
/// Provides scoped meter projections, tracked mutation records, and evidence persistence.
/// </summary>
public sealed class MeterRepository : GenericRepository<Meter>, IMeterRepository
{
    private readonly HavenDbContext _havenDbContext;
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the meter repository with shared EF and Dapper infrastructure.
    /// </summary>
    /// <param name="dbContext">The tracked Haven database context.</param>
    /// <param name="dapperService">The typed Dapper read service.</param>
    public MeterRepository(HavenDbContext dbContext, IDapperService dapperService)
        : base(dbContext)
    {
        _havenDbContext = dbContext;
        _dapperService = dapperService;
    }

    /// <summary>
    /// Resolves internal room and property identifiers inside the current landlord scope.
    /// </summary>
    /// <param name="roomPublicId">The public room identifier.</param>
    /// <param name="currentPartyId">The current landlord party identifier.</param>
    /// <param name="relationshipCodes">The relationship codes allowed for the scope.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The resolved scope, or <see langword="null"/> when it is outside the landlord scope.</returns>
    public async Task<MeterScopeModel> GetScopeAsync(
        Guid roomPublicId,
        long currentPartyId,
        IReadOnlyCollection<string> relationshipCodes,
        CancellationToken cancellationToken = default)
    {
        // Read-only endpoints resolve public scope through Dapper before querying bounded meter projections.
        return await _dapperService.QueryFirstOrDefaultAsync<MeterScopeModel>(
            InfrastructureQueryConstants.GET_METER_SCOPE_QUERY,
            new
            {
                RoomPublicId = roomPublicId,
                CurrentPartyId = currentPartyId,
                RelationshipCodes = relationshipCodes.ToArray()
            },
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

    /// <summary>
    /// Loads selected and previous period rows as a read-only Dapper projection.
    /// </summary>
    /// <param name="parameters">The resolved room scope and calendar period boundaries.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The flat meter rows used to compose period detail.</returns>
    public async Task<IReadOnlyList<MeterRowModel>> GetPeriodRowsAsync(
        MeterPeriodQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // Return a flat projection so list/detail reads do not materialize tracked EF graphs.
        var rows = await _dapperService.QueryAsync<MeterRowModel>(
            InfrastructureQueryConstants.GET_METER_PERIOD_ROWS_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
        return rows.ToList();
    }

    /// <summary>
    /// Loads at most twelve monthly meter periods for one scoped room and year.
    /// </summary>
    /// <param name="parameters">The resolved room scope and calendar-year boundaries.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The monthly rows with current invoice payment state used to determine editability.</returns>
    public async Task<IReadOnlyList<MeterRowModel>> GetHistoryRowsAsync(
        MeterPeriodQueryParametersModel parameters,
        CancellationToken cancellationToken = default)
    {
        // History remains a bounded read model rather than an EF entity graph.
        var rows = await _dapperService.QueryAsync<MeterRowModel>(
            InfrastructureQueryConstants.GET_METER_HISTORY_ROWS_QUERY,
            parameters,
            DapperCommandOptionsHelper.CreateText(cancellationToken));
        return rows.ToList();
    }

    /// <summary>
    /// Prepares one landlord-scoped meter mutation using only the active EF transaction.
    /// </summary>
    /// <param name="request">The scope, period, status, and utility identifiers required by the mutation.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The prepared tracked state, or <see langword="null"/> when the room is outside landlord scope.</returns>
    public async Task<MeterMutationStateModel> GetMutationStateAsync(
        MeterMutationStateRequestModel request,
        CancellationToken cancellationToken = default)
    {
        var relationshipCodes = request.RelationshipCodes.ToArray();
        var lineTypeIds = new[] { request.ElectricLineTypeId, request.WaterLineTypeId };

        // Resolve authorization and lock the room through EF so the scope check shares the active write transaction.
        var room = await _havenDbContext.Units
            .FromSqlRaw(
                InfrastructureQueryConstants.GET_METER_MUTATION_SCOPE_QUERY,
                request.RoomPublicId,
                request.CurrentPartyId,
                relationshipCodes)
            .FirstOrDefaultAsync(cancellationToken);

        if (room is null)
        {
            return null;
        }

        // Load the display scope after the room lock; all remaining queries use its internal identifiers.
        var scope = await _havenDbContext.Properties
            .AsNoTracking()
            .Where(property => property.Id == room.PropertyId && !property.IsDeleted)
            .Select(property => new MeterScopeModel
            {
                PropertyId = property.Id,
                PropertyPublicId = property.PublicId,
                PropertyName = property.Name,
                UnitId = room.Id,
                RoomPublicId = room.PublicId,
                RoomName = room.UnitName
            })
            .SingleAsync(cancellationToken);

        // Keep current-period rows tracked because the service mutates them after existence guards pass.
        var meters = await _havenDbContext.Meters
            .Where(meter => meter.PropertyId == scope.PropertyId
                            && meter.UnitId == scope.UnitId
                            && meter.BillingPeriodFrom == request.PeriodFrom
                            && meter.BillingPeriodTo == request.PeriodTo
                            && !meter.IsDeleted)
            .ToListAsync(cancellationToken);

        // Resolve both utility policies in one bounded query; room overrides win during in-memory selection.
        var policies = await _havenDbContext.RentalChargePolicies
            .AsNoTracking()
            .Where(policy => policy.PropertyId == scope.PropertyId
                             && (policy.UnitId == scope.UnitId || policy.UnitId == null)
                             && lineTypeIds.Contains(policy.LineTypeId)
                             && policy.IsUsageBased
                             && !policy.IsDeleted)
            .Select(policy => new
            {
                policy.LineTypeId,
                policy.UnitId,
                Policy = new MeterPolicyModel
                {
                    Id = policy.Id,
                    Amount = policy.Amount
                }
            })
            .ToListAsync(cancellationToken);

        // Resolve the latest confirmed baseline for both utilities without issuing one query per type.
        var previousValues = await _havenDbContext.Meters
            .AsNoTracking()
            .Where(meter => meter.PropertyId == scope.PropertyId
                            && meter.UnitId == scope.UnitId
                            && lineTypeIds.Contains(meter.LineTypeId)
                            && meter.BillingPeriodFrom < request.PeriodFrom
                            && meter.StatusId == request.ConfirmedStatusId
                            && !meter.IsDeleted)
            .OrderByDescending(meter => meter.BillingPeriodFrom)
            .Select(meter => new
            {
                meter.LineTypeId,
                meter.CurrentReading
            })
            .ToListAsync(cancellationToken);

        var electricPolicy = policies
            .Where(policy => policy.LineTypeId == request.ElectricLineTypeId)
            .OrderByDescending(policy => policy.UnitId.HasValue)
            .Select(policy => policy.Policy)
            .FirstOrDefault();
        var waterPolicy = policies
            .Where(policy => policy.LineTypeId == request.WaterLineTypeId)
            .OrderByDescending(policy => policy.UnitId.HasValue)
            .Select(policy => policy.Policy)
            .FirstOrDefault();

        return new MeterMutationStateModel
        {
            Scope = scope,
            Electric = new MeterUtilityMutationStateModel
            {
                LineTypeId = request.ElectricLineTypeId,
                ExistingMeter = meters.FirstOrDefault(meter => meter.LineTypeId == request.ElectricLineTypeId),
                EffectivePolicy = electricPolicy,
                PreviousConfirmedValue = previousValues
                    .FirstOrDefault(value => value.LineTypeId == request.ElectricLineTypeId)
                    ?.CurrentReading
            },
            Water = new MeterUtilityMutationStateModel
            {
                LineTypeId = request.WaterLineTypeId,
                ExistingMeter = meters.FirstOrDefault(meter => meter.LineTypeId == request.WaterLineTypeId),
                EffectivePolicy = waterPolicy,
                PreviousConfirmedValue = previousValues
                    .FirstOrDefault(value => value.LineTypeId == request.WaterLineTypeId)
                    ?.CurrentReading
            }
        };
    }

    /// <summary>
    /// Loads evidence metadata for the selected meter records.
    /// </summary>
    /// <param name="meterIds">The internal meter record identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The evidence projections, or an empty collection when no meter IDs are supplied.</returns>
    public async Task<IReadOnlyList<MeterEvidenceRowModel>> GetEvidenceAsync(
        IReadOnlyCollection<long> meterIds,
        CancellationToken cancellationToken = default)
    {
        if (meterIds.Count == 0)
        {
            return [];
        }

        // Evidence is projected separately so meter history remains focused on period values.
        var rows = await _dapperService.QueryAsync<MeterEvidenceRowModel>(
            InfrastructureQueryConstants.GET_METER_EVIDENCE_QUERY,
            new { MeterIds = meterIds.ToArray() },
            DapperCommandOptionsHelper.CreateText(cancellationToken));

        return rows.ToList();
    }

    /// <summary>
    /// Loads tracked active evidence links for the selected room meter records.
    /// </summary>
    /// <param name="meterIds">The meter records that own the submitted evidence.</param>
    /// <param name="meterEntityTypeId">The meter entity-type discriminator.</param>
    /// <param name="meterPhotoLinkTypeId">The meter-photo link-type discriminator.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked links and their documents.</returns>
    public async Task<IReadOnlyList<DocumentLink>> GetEvidenceLinksForMutationAsync(
        IReadOnlyCollection<long> meterIds,
        long meterEntityTypeId,
        long meterPhotoLinkTypeId,
        CancellationToken cancellationToken = default)
    {
        if (meterIds.Count == 0)
        {
            return [];
        }

        // Match every polymorphic discriminator before tracking records so overlapping entity IDs cannot retire unrelated documents.
        return await _havenDbContext.DocumentLinks
            .Include(link => link.Document)
            .Where(link => meterIds.Contains(link.EntityId)
                           && link.EntityTypeId == meterEntityTypeId
                           && link.LinkTypeId.HasValue
                           && link.LinkTypeId.Value == meterPhotoLinkTypeId
                           && !link.IsDeleted
                           && !link.Document.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Stages uploaded evidence documents and their meter links.
    /// </summary>
    /// <param name="documents">The newly uploaded document records.</param>
    /// <param name="links">The links from each document to its meter record.</param>
    /// <param name="cancellationToken">The token used to cancel persistence.</param>
    /// <returns>A task that completes when both collections are staged in the active unit of work.</returns>
    public async Task AddEvidenceAsync(
        IReadOnlyCollection<Document> documents,
        IReadOnlyCollection<DocumentLink> links,
        CancellationToken cancellationToken = default)
    {
        // Stage documents and links together so the surrounding unit-of-work owns their commit.
        await _havenDbContext.Documents.AddRangeAsync(documents, cancellationToken);
        await _havenDbContext.DocumentLinks.AddRangeAsync(links, cancellationToken);
    }
}
