namespace Haven.Infrastructure.Repositories.Meters;

/// <summary>
/// Owns invoice graph loading and utility-line replacement persistence.
/// </summary>
public sealed class InvoiceUtilityRepository : IInvoiceUtilityRepository
{
    private readonly HavenDbContext _dbContext;
    private readonly IDapperService _dapperService;

    /// <summary>
    /// Creates the invoice utility repository.
    /// </summary>
    /// <param name="dbContext">The tracked Haven context.</param>
    /// <param name="dapperService">The Dapper service used only for read-only invoice impact previews.</param>
    public InvoiceUtilityRepository(HavenDbContext dbContext, IDapperService dapperService)
    {
        _dbContext = dbContext;
        _dapperService = dapperService;
    }

    /// <summary>
    /// Loads the tracked invoice graph for a room and billing period.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="periodFrom">The invoice period start.</param>
    /// <param name="periodTo">The invoice period end.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The latest non-deleted invoice graph, or <see langword="null"/>.</returns>
    public async Task<Invoice> GetInvoiceGraphAsync(
        long unitId,
        DateOnly periodFrom,
        DateOnly periodTo,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Lock the latest matching invoice identifier on the required EF mutation transaction.
        var invoiceId = await _dapperService.ExecuteScalarAsync<long?>(
            InfrastructureQueryConstants.GET_INVOICE_ID_FOR_UPDATE_QUERY,
            new { UnitId = unitId, PeriodFrom = periodFrom, PeriodTo = periodTo },
            DapperCommandOptionsHelper.CreateTransactionalText(
                _dbContext,
                cancellationToken));

        if (!invoiceId.HasValue)
        {
            return null;
        }

        // Step 2: Load only the tracked graph needed to replace utility lines or create a replacement invoice.
        return await _dbContext.Invoices
            .Include(invoice => invoice.Contract)
            .Include(invoice => invoice.InvoiceLines)
                .ThenInclude(line => line.Utility)
            .FirstOrDefaultAsync(invoice => invoice.Id == invoiceId.Value, cancellationToken);
    }

    /// <summary>
    /// Checks whether an invoice has any non-deleted allocation backed by a successful payment.
    /// </summary>
    /// <param name="invoiceId">The internal invoice identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><see langword="true"/> when a successful payment allocation exists.</returns>
    public async Task<bool> HasSuccessfulPaymentAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        // The payment guard must observe the same transaction snapshot and locks as the invoice mutation.
        return await _dapperService.ExecuteScalarAsync<bool>(
            InfrastructureQueryConstants.HAS_SUCCESSFUL_INVOICE_PAYMENT_QUERY,
            new
            {
                InvoiceId = invoiceId,
                SuccessfulPaymentStatusCode = MASTER_CODE_PAYMENT_STATUS_SUCCESS,
                PaymentStatusTypeCode = MasterDataTypeEnum.PaymentStatus.ToString()
            },
            DapperCommandOptionsHelper.CreateTransactionalText(
                _dbContext,
                cancellationToken));
    }

    /// <summary>
    /// Loads the invoice state for the period that will receive confirmed utility charges.
    /// </summary>
    /// <param name="unitId">The internal room identifier.</param>
    /// <param name="periodFrom">The invoice period start.</param>
    /// <param name="periodTo">The invoice period end.</param>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The latest invoice impact row, or <see langword="null"/> when no invoice exists.</returns>
    public Task<MeterInvoiceImpactRowModel> GetInvoiceImpactAsync(
        long unitId,
        DateOnly periodFrom,
        DateOnly periodTo,
        CancellationToken cancellationToken = default)
    {
        // The preflight projection mirrors the mutation guard, including only successful payment allocations.
        return _dapperService.QueryFirstOrDefaultAsync<MeterInvoiceImpactRowModel>(
            InfrastructureQueryConstants.GET_METER_INVOICE_IMPACT_QUERY,
            new
            {
                UnitId = unitId,
                PeriodFrom = periodFrom,
                PeriodTo = periodTo,
                SuccessfulPaymentStatusCode = MASTER_CODE_PAYMENT_STATUS_SUCCESS,
                PaymentStatusTypeCode = MasterDataTypeEnum.PaymentStatus.ToString()
            },
            DapperCommandOptionsHelper.CreateText(cancellationToken));
    }

    /// <summary>
    /// Replaces utility-backed lines while preserving recurring invoice lines.
    /// </summary>
    /// <param name="invoice">The tracked invoice receiving the utility lines.</param>
    /// <param name="meters">The confirmed meter records used to rebuild utility charges.</param>
    /// <param name="chargeModeId">The meter-reading charge mode identifier.</param>
    /// <param name="cancellationToken">The token reserved for repository contract consistency.</param>
    /// <returns>A completed task after the tracked graph is updated.</returns>
    public Task ReplaceUtilityLinesAsync(
        Invoice invoice,
        IReadOnlyCollection<Meter> meters,
        long chargeModeId,
        CancellationToken cancellationToken = default)
    {
        // Retire only utility-backed lines; rent, package, parking, and recurring lines remain untouched.
        foreach (var line in invoice.InvoiceLines.Where(line => line.Utility is not null && !line.IsDeleted))
        {
            line.IsDeleted = true;
            line.Utility.IsDeleted = true;
        }

        foreach (var meter in meters)
        {
            var amount = (meter.UsageQuantity ?? 0) * (meter.UnitPriceSnapshot ?? 0);
            invoice.InvoiceLines.Add(new InvoiceLine
            {
                PublicId = Guid.NewGuid(),
                LineTypeId = meter.LineTypeId,
                ChargePolicyId = meter.ChargePolicyId,
                Description = UTILITY_INVOICE_LINE_DESCRIPTION,
                Amount = amount,
                Utility = new InvoiceLineUtility
                {
                    PublicId = Guid.NewGuid(),
                    ChargeModeId = chargeModeId,
                    Meter = meter,
                    ReadingDate = meter.ReadingDate,
                    PreviousReading = meter.PreviousReading,
                    CurrentReading = meter.CurrentReading,
                    UsageQuantity = meter.UsageQuantity,
                    UnitPrice = meter.UnitPriceSnapshot
                }
            });
        }

        invoice.TotalAmount = invoice.InvoiceLines.Where(line => !line.IsDeleted).Sum(line => line.Amount);
        invoice.BalanceAmount = invoice.TotalAmount - invoice.PaidAmount;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a draft replacement invoice while preserving non-utility charges.
    /// </summary>
    /// <param name="source">The issued or overdue invoice being replaced.</param>
    /// <param name="draftStatusId">The master-data identifier for draft invoice status.</param>
    /// <param name="meters">The confirmed meter records used to rebuild utility lines.</param>
    /// <param name="chargeModeId">The charge mode stored on replacement invoice lines.</param>
    /// <param name="cancellationToken">The token used to cancel persistence work.</param>
    /// <returns>The staged replacement invoice graph.</returns>
    public async Task<Invoice> CreateReplacementAsync(
        Invoice source,
        long draftStatusId,
        IReadOnlyCollection<Meter> meters,
        long chargeModeId,
        CancellationToken cancellationToken = default)
    {
        // Keep the issued source immutable and create a new draft graph with explicit lineage.
        var replacement = new Invoice
        {
            PublicId = Guid.NewGuid(),
            InvoiceCode = CodeGenerationHelper.GenerateCode(INVOICE_CODE_PREFIX, INVOICE_CODE_RANDOM_LENGTH),
            ContractId = source.ContractId,
            BillToPartyId = source.BillToPartyId,
            InvoiceTypeId = source.InvoiceTypeId,
            BillingPeriodFrom = source.BillingPeriodFrom,
            BillingPeriodTo = source.BillingPeriodTo,
            DueDate = source.DueDate,
            StatusId = draftStatusId,
            ReplacesInvoice = source
        };

        // Copy only non-utility lines before rebuilding electric and water snapshots from confirmed readings.
        foreach (var sourceLine in source.InvoiceLines.Where(line => line.Utility is null && !line.IsDeleted))
        {
            replacement.InvoiceLines.Add(new InvoiceLine
            {
                PublicId = Guid.NewGuid(),
                LineTypeId = sourceLine.LineTypeId,
                ChargePolicyId = sourceLine.ChargePolicyId,
                PartyVehicleId = sourceLine.PartyVehicleId,
                Description = sourceLine.Description,
                Amount = sourceLine.Amount,
                Note = sourceLine.Note
            });
        }

        await ReplaceUtilityLinesAsync(replacement, meters, chargeModeId, cancellationToken);
        await _dbContext.Invoices.AddAsync(replacement, cancellationToken);
        return replacement;
    }

}
