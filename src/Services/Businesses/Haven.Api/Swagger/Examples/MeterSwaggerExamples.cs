namespace Haven.Api.Swagger.Examples;

/// <summary>
/// Groups room-meter form, response, route, and query examples.
/// </summary>
internal static class MeterSwaggerExamples
{
    /// <summary>
    /// Provides room-meter route and query values.
    /// </summary>
    internal sealed class Values : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds room-meter route and calendar query examples.
        /// </summary>
        /// <returns>The room-meter route and query values.</returns>
        protected override object BuildExample() => new { roomId = HavenSwaggerExampleConstants.ROOM_ID, month = 7, year = 2026 };
    }

    /// <summary>
    /// Provides create-meter multipart metadata without binary file values.
    /// </summary>
    internal sealed class CreateValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds create-meter values while leaving evidence file pickers empty.
        /// </summary>
        /// <returns>The create-meter metadata fields.</returns>
        protected override object BuildExample() => new { roomId = HavenSwaggerExampleConstants.ROOM_ID, billingDate = "2026-07-01", electricPrevious = 1200m, electricCurrent = 1325m, electricPrice = 3500m, waterPrevious = 310m, waterCurrent = 326m, waterPrice = 18000m };
    }

    /// <summary>
    /// Provides update-meter multipart metadata without binary file values.
    /// </summary>
    internal sealed class UpdateValues : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds update-meter values while leaving evidence file pickers empty.
        /// </summary>
        /// <returns>The update-meter metadata fields.</returns>
        protected override object BuildExample() => new { roomId = HavenSwaggerExampleConstants.ROOM_ID, billingDate = "2026-07-01", electricPrevious = 1200m, electricCurrent = 1328m, electricPrice = 3500m, waterPrevious = 310m, waterCurrent = 327m, waterPrice = 18000m, deletedElectricImageIds = Array.Empty<string>(), deletedWaterImageIds = Array.Empty<string>() };
    }

    /// <summary>
    /// Provides a meter-history success envelope.
    /// </summary>
    internal sealed class HistoryResponse : SwaggerSuccessExampleProvider<IReadOnlyList<MeterHistoryItemResponseDto>>
    {
        /// <summary>
        /// Builds the bounded room-meter history response.
        /// </summary>
        /// <returns>The meter-history success envelope.</returns>
        protected override IReadOnlyList<MeterHistoryItemResponseDto> BuildData() =>
        [
            new MeterHistoryItemResponseDto
            {
                Month = 7,
                Year = 2026,
                Date = new DateOnly(2026, 7, 1),
                Electric = BuildMeterValue(1200m, 1325m, 3500m),
                Water = BuildMeterValue(310m, 326m, 18000m),
                CanEdit = true
            }
        ];
    }

    /// <summary>
    /// Provides a meter-period success envelope.
    /// </summary>
    internal sealed class PeriodResponse : SwaggerSuccessExampleProvider<MeterPeriodDetailResponseDto>
    {
        /// <summary>
        /// Builds one room-meter period response with live invoice state.
        /// </summary>
        /// <returns>The meter-period success envelope.</returns>
        protected override MeterPeriodDetailResponseDto BuildData() => new()
        {
            Property = new MeterScopeResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID), Name = "Haven Nguyen Hue" },
            Room = new MeterScopeResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID), Name = "101" },
            CurrentPeriod = new MeterPeriodResponseDto
            {
                Month = 7,
                Year = 2026,
                Electric = BuildMeterValue(1200m, 1325m, 3500m),
                Water = BuildMeterValue(310m, 326m, 18000m)
            },
            InvoiceImpact = new MeterInvoiceImpactResponseDto { ActionCode = "CREATE", CanSave = true }
        };
    }

    /// <summary>
    /// Builds one calculated meter value for history and detail examples.
    /// </summary>
    /// <param name="previous">The previous meter value.</param>
    /// <param name="current">The current meter value.</param>
    /// <param name="price">The confirmed unit price.</param>
    /// <returns>The calculated meter example.</returns>
    private static MeterValueResponseDto BuildMeterValue(decimal previous, decimal current, decimal price) => new()
    {
        Previous = previous,
        Current = current,
        UsageQuantity = current - previous,
        Price = price,
        Amount = (current - previous) * price,
        Date = new DateOnly(2026, 7, 1),
        StatusCode = "CONFIRMED"
    };
}
