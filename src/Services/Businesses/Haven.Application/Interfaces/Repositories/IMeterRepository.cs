namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides scoped meter read projections and tracked mutation operations.
/// </summary>
public interface IMeterRepository : IGenericRepository<Meter>
{
    /// <summary>
    /// Resolves one room inside the active landlord scope for a read projection.
    /// </summary>
    /// <param name="roomPublicId">The public room identifier.</param>
    /// <param name="currentPartyId">The current landlord party identifier.</param>
    /// <param name="relationshipCodes">The active relationship codes allowed to manage the property.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The resolved internal scope, or <see langword="null"/> when access is unavailable.</returns>
    Task<MeterScopeModel> GetScopeAsync(
        Guid roomPublicId,
        long currentPartyId,
        IReadOnlyCollection<string> relationshipCodes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the selected and previous meter periods for one scoped room.
    /// </summary>
    /// <param name="parameters">The resolved scope and period boundaries.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The flat electricity and water rows used to compose period detail.</returns>
    Task<IReadOnlyList<MeterRowModel>> GetPeriodRowsAsync(MeterPeriodQueryParametersModel parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads meter history by calendar month for one scoped room.
    /// </summary>
    /// <param name="parameters">The resolved scope and calendar-year boundaries.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The flat monthly reading rows with current invoice editability state.</returns>
    Task<IReadOnlyList<MeterRowModel>> GetHistoryRowsAsync(MeterPeriodQueryParametersModel parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares the complete EF-backed state required to mutate one room meter period.
    /// </summary>
    /// <param name="request">The landlord scope, period, status, and utility identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The prepared mutation state, or <see langword="null"/> when the room is outside landlord scope.</returns>
    Task<MeterMutationStateModel> GetMutationStateAsync(
        MeterMutationStateRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads evidence projections for selected meter records.
    /// </summary>
    /// <param name="meterIds">The internal meter record identifiers.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The evidence rows keyed to meter records.</returns>
    Task<IReadOnlyList<MeterEvidenceRowModel>> GetEvidenceAsync(
        IReadOnlyCollection<long> meterIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads tracked active evidence links for the selected meter records.
    /// </summary>
    /// <param name="meterIds">The internal meter record identifiers that own the evidence.</param>
    /// <param name="meterEntityTypeId">The meter entity-type discriminator.</param>
    /// <param name="meterPhotoLinkTypeId">The meter-photo link-type discriminator.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked document links and documents.</returns>
    Task<IReadOnlyList<DocumentLink>> GetEvidenceLinksForMutationAsync(
        IReadOnlyCollection<long> meterIds,
        long meterEntityTypeId,
        long meterPhotoLinkTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages new evidence documents and links in the active unit of work.
    /// </summary>
    /// <param name="documents">The uploaded document records.</param>
    /// <param name="links">The links from documents to meter records.</param>
    /// <param name="cancellationToken">The token used to cancel persistence.</param>
    /// <returns>A task that completes after the records are staged.</returns>
    Task AddEvidenceAsync(IReadOnlyCollection<Document> documents, IReadOnlyCollection<DocumentLink> links, CancellationToken cancellationToken = default);
}
