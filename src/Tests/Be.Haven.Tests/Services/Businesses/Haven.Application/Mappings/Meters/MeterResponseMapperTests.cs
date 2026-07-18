using Haven.Application.Mappings.Meters;
using Haven.Application.Models.Meters.Rows;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings.Meters;

public sealed class MeterResponseMapperTests
{
    [Fact]
    public void MapPeriodDetail_Should_ComposePeriodsAndAttachEvidence()
    {
        // Arrange
        var meterId = 42L;
        var currentFrom = new DateOnly(2026, 7, 1);
        var previousFrom = currentFrom.AddMonths(-1);
        var rows = new List<MeterRowModel>
        {
            new()
            {
                MeterId = meterId,
                MeterPublicId = Guid.NewGuid(),
                BillingPeriodFrom = currentFrom,
                LineTypeCode = MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC,
                PreviousReading = 100,
                CurrentReading = 125,
                UsageQuantity = 25,
                UnitPriceSnapshot = 3_500
            }
        };
        var evidence = new Dictionary<long, IReadOnlyList<MeterEvidenceRowModel>>
        {
            [meterId] =
            [
                new()
                {
                    MeterId = meterId,
                    EvidencePublicId = Guid.NewGuid(),
                    Url = "https://example.test/electric-1.jpg",
                    ContentType = "image/jpeg",
                    SortOrder = 1
                },
                new()
                {
                    MeterId = meterId,
                    EvidencePublicId = Guid.NewGuid(),
                    Url = "https://example.test/electric-2.jpg",
                    ContentType = "image/jpeg",
                    SortOrder = 2
                }
            ]
        };

        // Act
        var response = MeterResponseMapper.MapPeriodDetail(
            new MeterScopeModel
            {
                PropertyPublicId = Guid.NewGuid(),
                PropertyName = "Atlas Plaza",
                RoomPublicId = Guid.NewGuid(),
                RoomName = "Room 101"
            },
            currentFrom,
            previousFrom,
            rows,
            evidence);

        // Assert
        response.CurrentPeriod.Electric.Amount.Should().Be(87_500);
        response.CurrentPeriod.Electric.Evidence.Should().HaveCount(2);
        response.CurrentPeriod.Electric.Evidence[0].Url.Should().Be("https://example.test/electric-1.jpg");
        response.PreviousPeriod.Month.Should().Be(6);
    }
}
