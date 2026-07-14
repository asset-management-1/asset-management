namespace Haven.Application.Mappings.Meters;

/// <summary>
/// Composes flat meter read rows and evidence projections into API response graphs.
/// </summary>
public static class MeterResponseMapper
{
    /// <summary>
    /// Builds the selected and previous room meter periods.
    /// </summary>
    /// <param name="scope">The room and property display scope.</param>
    /// <param name="currentPeriodFrom">The first date of the selected month.</param>
    /// <param name="previousPeriodFrom">The first date of the preceding month.</param>
    /// <param name="rows">The flat meter rows for both periods.</param>
    /// <param name="evidence">The evidence rows keyed by internal meter identifier.</param>
    /// <returns>The composed period response.</returns>
    public static MeterPeriodDetailResponseDto MapPeriodDetail(
        MeterScopeModel scope,
        DateOnly currentPeriodFrom,
        DateOnly previousPeriodFrom,
        IReadOnlyCollection<MeterRowModel> rows,
        IReadOnlyDictionary<long, IReadOnlyList<MeterEvidenceRowModel>> evidence)
    {
        var currentRows = rows
            .Where(row => row.BillingPeriodFrom == currentPeriodFrom)
            .ToList();
        var previousRows = rows
            .Where(row => row.BillingPeriodFrom == previousPeriodFrom)
            .ToList();

        // Reconstruct the two-month response only after the repository has returned its flat projections.
        return new MeterPeriodDetailResponseDto
        {
            Property = new MeterScopeResponseDto
            {
                Id = scope.PropertyPublicId,
                Name = scope.PropertyName
            },
            Room = new MeterScopeResponseDto
            {
                Id = scope.RoomPublicId,
                Name = scope.RoomName
            },
            CurrentPeriod = MapPeriod(currentPeriodFrom, currentRows, evidence),
            PreviousPeriod = MapPeriod(previousPeriodFrom, previousRows, evidence)
        };
    }

    /// <summary>
    /// Builds one room meter history item and its current invoice editability display.
    /// </summary>
    /// <param name="periodFrom">The first date of the history month.</param>
    /// <param name="rows">The electricity and water rows in that month.</param>
    /// <param name="evidence">The evidence rows keyed by internal meter identifier.</param>
    /// <returns>The composed history item.</returns>
    public static MeterHistoryItemResponseDto MapHistoryItem(
        DateOnly periodFrom,
        IReadOnlyCollection<MeterRowModel> rows,
        IReadOnlyDictionary<long, IReadOnlyList<MeterEvidenceRowModel>> evidence)
    {
        var electric = MapValue(
            rows.FirstOrDefault(row => row.LineTypeCode == MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC),
            evidence);
        var water = MapValue(
            rows.FirstOrDefault(row => row.LineTypeCode == MASTER_CODE_INVOICE_LINE_TYPE_WATER),
            evidence);
        var isInvoicePaid = rows.Any(row => row.HasInvoicePayment
                                            || row.InvoicePaidAmount > 0
                                            || row.InvoiceStatusCode is MASTER_CODE_INVOICE_STATUS_PARTIALLY_PAID
                                                or MASTER_CODE_INVOICE_STATUS_PAID);

        return new MeterHistoryItemResponseDto
        {
            Month = periodFrom.Month,
            Year = periodFrom.Year,
            Date = rows.FirstOrDefault()?.ReadingDate,
            Electric = electric,
            Water = water,
            CanEdit = !isInvoicePaid,
            EditBlockedReasonCode = isInvoicePaid
                ? ApplicationErrorConstants.MeterErrors.ERROR_METER_MUTATION_BLOCKED
                : null
        };
    }

    /// <summary>
    /// Builds one monthly electricity and water response.
    /// </summary>
    /// <param name="periodFrom">The first date of the represented month.</param>
    /// <param name="rows">The electricity and water rows in that month.</param>
    /// <param name="evidence">The evidence rows keyed by internal meter identifier.</param>
    /// <returns>The monthly meter values.</returns>
    private static MeterPeriodResponseDto MapPeriod(
        DateOnly periodFrom,
        IReadOnlyCollection<MeterRowModel> rows,
        IReadOnlyDictionary<long, IReadOnlyList<MeterEvidenceRowModel>> evidence)
    {
        return new MeterPeriodResponseDto
        {
            Month = periodFrom.Month,
            Year = periodFrom.Year,
            Electric = MapValue(
                rows.FirstOrDefault(row => row.LineTypeCode == MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC),
                evidence),
            Water = MapValue(
                rows.FirstOrDefault(row => row.LineTypeCode == MASTER_CODE_INVOICE_LINE_TYPE_WATER),
                evidence)
        };
    }

    /// <summary>
    /// Maps one persisted meter row and attaches its evidence collection.
    /// </summary>
    /// <param name="row">The persisted meter row, or <see langword="null"/> when the utility is absent.</param>
    /// <param name="evidence">The evidence rows keyed by internal meter identifier.</param>
    /// <returns>The mapped value, or <see langword="null"/> when no row exists.</returns>
    private static MeterValueResponseDto MapValue(
        MeterRowModel row,
        IReadOnlyDictionary<long, IReadOnlyList<MeterEvidenceRowModel>> evidence)
    {
        if (row is null)
        {
            return null;
        }

        var response = row.Adapt<MeterValueResponseDto>();
        response.Amount = row.UsageQuantity * row.UnitPriceSnapshot;

        // Evidence is loaded independently from the meter projection and attached by its internal meter ID.
        if (row.MeterId.HasValue && evidence.TryGetValue(row.MeterId.Value, out var evidenceRows))
        {
            response.Evidence = evidenceRows
                .OrderBy(evidenceRow => evidenceRow.SortOrder)
                .Select(evidenceRow => new MeterEvidenceResponseDto
                {
                    Id = evidenceRow.EvidencePublicId,
                    Url = evidenceRow.Url,
                    ContentType = evidenceRow.ContentType
                })
                .ToList();
        }

        return response;
    }
}
