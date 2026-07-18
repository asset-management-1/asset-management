using Authentication.Infrastructure.Constants;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Constants;

/// <summary>
/// Verifies security-sensitive predicates in authentication read SQL.
/// </summary>
public sealed class InfrastructureQueryConstantsTests
{
    /// <summary>
    /// Ensures user-info exposes only active Party contexts with canonical master-data codes.
    /// </summary>
    [Fact]
    public void GetUserInfoQuery_Should_UseCanonicalActivePartyPredicates()
    {
        var query = InfrastructureQueryConstants.GET_USER_INFO_RESPONSE_BY_PUBLIC_ID_QUERY;

        query.Should().Contain("current_party_status.\"Code\" = 'ACTIVE'");
        query.Should().Contain("current_party_status_type.\"Code\" = 'PartyStatus'");
        query.Should().Contain("party_status.\"Code\" = 'ACTIVE'");
        query.Should().Contain("party_type_definition.\"Code\" = 'PartyType'");
        query.Should().NotContain("COALESCE(NULLIF");
    }
}
