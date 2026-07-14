namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Defines party-identifier repository operations.
/// </summary>
public interface IPartyIdentifierRepository : IGenericRepository<PartyIdentifier>
{
    /// <summary>
    /// Loads one identifier by type and identifier value.
    /// </summary>
    /// <param name="identifierTypeId">The identifier type master-data value id.</param>
    /// <param name="identifierValue">The submitted identifier value.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The matched identifier; otherwise <c>null</c>.</returns>
    Task<PartyIdentifier> GetByTypeAndValueAsync(
        long identifierTypeId,
        string identifierValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads active identifiers of the supplied types for the supplied parties.
    /// </summary>
    /// <param name="partyIds">The linked party ids to inspect.</param>
    /// <param name="identifierTypeIds">The identifier type master-data value ids.</param>
    /// <param name="cancellationToken">The token used to cancel the database operation.</param>
    /// <returns>The active identifiers attached to the supplied parties.</returns>
    Task<IReadOnlyList<PartyIdentifier>> GetTrackedByPartyIdsAndTypesAsync(
        IReadOnlyCollection<long> partyIds,
        IReadOnlyCollection<long> identifierTypeIds,
        CancellationToken cancellationToken = default);
}
