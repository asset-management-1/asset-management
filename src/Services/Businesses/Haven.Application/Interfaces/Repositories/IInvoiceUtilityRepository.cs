namespace Haven.Application.Interfaces.Repositories;

/// <summary>
/// Provides invoice preflight and utility-line synchronization operations.
/// </summary>
public interface IInvoiceUtilityRepository
{
    /// <summary>
    /// Locks and loads the latest invoice graph affected by one room meter period.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="periodFrom">The invoice period start.</param>
    /// <param name="periodTo">The invoice period end.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The tracked invoice graph, or <see langword="null"/> when billing has not run.</returns>
    Task<Invoice> GetInvoiceGraphAsync(long unitId, DateOnly periodFrom, DateOnly periodTo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for an active payment allocation on the locked invoice.
    /// </summary>
    /// <param name="invoiceId">The internal invoice identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><see langword="true"/> when payment has begun.</returns>
    Task<bool> HasSuccessfulPaymentAsync(long invoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads read-only invoice impact for the utility period screen.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="periodFrom">The invoice period start.</param>
    /// <param name="periodTo">The invoice period end.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The latest invoice state, or <see langword="null"/> when no invoice exists.</returns>
    Task<MeterInvoiceImpactRowModel> GetInvoiceImpactAsync(long unitId, DateOnly periodFrom, DateOnly periodTo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces utility-backed lines while preserving recurring invoice lines.
    /// </summary>
    /// <param name="invoice">The tracked draft invoice.</param>
    /// <param name="meters">The confirmed meter records used to rebuild utility charges.</param>
    /// <param name="chargeModeId">The meter-reading charge-mode identifier.</param>
    /// <param name="cancellationToken">The token used to cancel persistence.</param>
    /// <returns>A task that completes after the graph is updated.</returns>
    Task ReplaceUtilityLinesAsync(
        Invoice invoice,
        IReadOnlyCollection<Meter> meters,
        long chargeModeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a draft replacement while cancelling an unpaid issued invoice.
    /// </summary>
    /// <param name="source">The tracked issued or overdue invoice.</param>
    /// <param name="draftStatusId">The draft invoice status identifier.</param>
    /// <param name="meters">The confirmed meter records used to rebuild utility charges.</param>
    /// <param name="chargeModeId">The meter-reading charge-mode identifier.</param>
    /// <param name="cancellationToken">The token used to cancel persistence.</param>
    /// <returns>The staged replacement invoice.</returns>
    Task<Invoice> CreateReplacementAsync(
        Invoice source,
        long draftStatusId,
        IReadOnlyCollection<Meter> meters,
        long chargeModeId,
        CancellationToken cancellationToken = default);
}
