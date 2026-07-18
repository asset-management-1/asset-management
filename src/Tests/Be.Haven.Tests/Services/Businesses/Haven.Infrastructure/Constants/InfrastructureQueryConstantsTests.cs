using System.Reflection;
using Haven.Infrastructure.Constants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Constants;

public sealed class InfrastructureQueryConstantsTests
{
    /// <summary>
    /// Ensures every property scope observes the complete effective relationship period.
    /// </summary>
    [Fact]
    public void PropertyScopedQueries_Should_RequireStartedAndUnexpiredRelationship()
    {
        // Discover every SQL constant that authorizes through asset.PropertyParties.
        var propertyScopedQueries = typeof(InfrastructureQueryConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => new
            {
                field.Name,
                Sql = (string)field.GetRawConstantValue()!
            })
            .Where(query => query.Sql.Contains(
                "\"asset\".\"PropertyParties\"",
                StringComparison.Ordinal))
            .ToArray();

        propertyScopedQueries.Should().NotBeEmpty();

        // Report every query missing either side of the effective-date boundary in one assertion.
        var queriesWithIncompleteEffectivePeriod = propertyScopedQueries
            .Where(query => !query.Sql.Contains(
                                "property_party.\"StartDate\" IS NULL OR property_party.\"StartDate\" <= CURRENT_DATE",
                                StringComparison.Ordinal)
                            || !query.Sql.Contains(
                                "property_party.\"EndDate\" IS NULL OR property_party.\"EndDate\" >= CURRENT_DATE",
                                StringComparison.Ordinal))
            .Select(query => query.Name)
            .ToArray();

        queriesWithIncompleteEffectivePeriod.Should().BeEmpty(
            "every landlord-scoped query must reject relationships that have not started or have expired");
    }

    /// <summary>
    /// Ensures property reads expose persisted structure totals instead of hiding drift with live-count fallbacks.
    /// </summary>
    [Fact]
    public void PropertyReads_Should_UsePersistedStructureTotals()
    {
        // The aggregate is recomputed by write flows; reads must not silently substitute a second source of truth.
        InfrastructureQueryConstants.GET_PROPERTY_LIST_PAGE_QUERY
            .Should().NotContain("NULLIF(property.\"TotalFloors\", 0)");
        InfrastructureQueryConstants.GET_PROPERTY_LIST_PAGE_QUERY
            .Should().NotContain("NULLIF(property.\"TotalUnits\", 0)");
        InfrastructureQueryConstants.GET_PROPERTY_DETAIL_HEADER_QUERY
            .Should().NotContain("summary.\"TotalFloors\"");
        InfrastructureQueryConstants.GET_PROPERTY_DETAIL_HEADER_QUERY
            .Should().NotContain("summary.\"TotalUnits\"");
    }

    /// <summary>
    /// Ensures meter invoice guards count only payments with the canonical successful status.
    /// </summary>
    [Fact]
    public void MeterInvoiceImpact_Should_FilterSuccessfulPaymentsByCanonicalCode()
    {
        // Payment allocations alone do not prove collection because failed and cancelled attempts remain persisted.
        var query = InfrastructureQueryConstants.GET_METER_INVOICE_IMPACT_QUERY;

        query.Should().Contain("payment_status.\"Code\" = @SuccessfulPaymentStatusCode");
        query.Should().Contain("payment_status_type.\"Code\" = @PaymentStatusTypeCode");
    }
}
