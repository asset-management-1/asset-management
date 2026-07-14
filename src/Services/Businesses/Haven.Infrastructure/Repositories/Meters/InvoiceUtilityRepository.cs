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
        // Lock the latest matching invoice through the active EF transaction before payment and status guards run.
        var invoiceId = await _dbContext.Database.SqlQueryRaw<long>(
                """
                SELECT invoice."Id" AS "Value"
                FROM "billing"."Invoices" invoice
                INNER JOIN "leasing"."Contracts" contract ON contract."Id" = invoice."ContractId"
                WHERE contract."UnitId" = {0}
                  AND invoice."BillingPeriodFrom" = {1}
                  AND invoice."BillingPeriodTo" = {2}
                  AND invoice."IsDeleted" = FALSE
                ORDER BY invoice."Id" DESC
                LIMIT 1
                FOR UPDATE
                """,
                unitId,
                periodFrom,
                periodTo)
            .SingleOrDefaultAsync(cancellationToken);

        if (invoiceId == 0)
        {
            return null;
        }

        // Load only the tracked graph needed to replace utility lines or create a replacement invoice.
        return await _dbContext.Invoices
            .Include(invoice => invoice.Contract)
            .Include(invoice => invoice.InvoiceLines)
                .ThenInclude(line => line.Utility)
            .FirstOrDefaultAsync(invoice => invoice.Id == invoiceId, cancellationToken);
    }

    /// <summary>
    /// Checks whether an invoice has any non-deleted payment allocation.
    /// </summary>
    /// <param name="invoiceId">The internal invoice identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns><see langword="true"/> when a payment allocation exists.</returns>
    public Task<bool> HasSuccessfulPaymentAsync(long invoiceId, CancellationToken cancellationToken = default)
    {
        // Run the payment existence check on the EF connection so it participates in the caller transaction.
        return _dbContext.Database.SqlQueryRaw<bool>(
            """
            SELECT EXISTS (
                SELECT 1 FROM "billing"."PaymentAllocations" allocation
                INNER JOIN "billing"."Payments" payment ON payment."Id" = allocation."PaymentId"
                WHERE allocation."InvoiceId" = {0}
                  AND allocation."IsDeleted" = FALSE AND payment."IsDeleted" = FALSE
            ) AS "Value"
            """,
            invoiceId)
            .SingleAsync(cancellationToken);
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
        // Only invoice status and paid amount are needed for the preflight projection.
        return _dapperService.QueryFirstOrDefaultAsync<MeterInvoiceImpactRowModel>(
            InfrastructureQueryConstants.GET_METER_INVOICE_IMPACT_QUERY,
            new { UnitId = unitId, PeriodFrom = periodFrom, PeriodTo = periodTo },
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
