using Haven.Application.Mappings.Tenants;
using Haven.Application.Models.Tenants.Common;
using Haven.Domain.Entities;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings;

public sealed class TenantMappingTests
{
    static TenantMappingTests()
    {
    }

    [Fact]
    public void TenantProfile_Should_MapOnlySubmittedProfileFields()
    {
        var profile = new TenantProfileModel
        {
            FullName = "Nguyen Van A",
            Phone = "0901234567",
            Email = "tenant@haven.test"
        };

        var party = profile.Adapt<Party>();

        party.DisplayName.Should().Be(profile.FullName);
        party.PrimaryPhone.Should().Be(profile.Phone);
        party.PrimaryEmail.Should().Be(profile.Email);
        party.PublicId.Should().Be(Guid.Empty);
        party.PartyTypeId.Should().Be(0);
        party.StatusId.Should().Be(0);
    }
}
