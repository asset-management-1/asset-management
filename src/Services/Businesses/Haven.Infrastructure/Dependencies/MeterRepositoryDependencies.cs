namespace Haven.Infrastructure.Dependencies;

/// <summary>
/// Groups repositories used by room meter read and invoice synchronisation workflows.
/// </summary>
public sealed class MeterRepositoryDependencies
{
    /// <summary>
    /// Creates the grouped meter repositories.
    /// </summary>
    /// <param name="meterRepository">The repository for meter scope, projections, and tracked records.</param>
    /// <param name="invoiceUtilityRepository">The repository for invoice impact and utility-line persistence.</param>
    public MeterRepositoryDependencies(
        IMeterRepository meterRepository,
        IInvoiceUtilityRepository invoiceUtilityRepository)
    {
        MeterRepository = meterRepository;
        InvoiceUtilityRepository = invoiceUtilityRepository;
    }

    /// <summary>
    /// Gets the room meter repository.
    /// </summary>
    public IMeterRepository MeterRepository { get; }

    /// <summary>
    /// Gets the invoice utility repository.
    /// </summary>
    public IInvoiceUtilityRepository InvoiceUtilityRepository { get; }
}
