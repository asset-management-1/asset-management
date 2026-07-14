using Authentication.Application.Helpers;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Helpers;

public sealed class AuthenticationFlowHelperTests
{
    [Fact]
    public void CanLogin_Should_UseCanonicalStatusCode_NotDisplayName()
    {
        var user = new User
        {
            EmailConfirmed = true,
            Status = new MasterDataValue
            {
                Code = "SUSPENDED",
                Name = "ACTIVE"
            }
        };

        var result = AuthenticationFlowHelper.CanLogin(user);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("TENANT", PartyTypeEnum.Tenant)]
    [InlineData("LANDLORD", PartyTypeEnum.Landlord)]
    public void ToPartyType_Should_MapCanonicalCodes(string code, PartyTypeEnum expected)
    {
        ApiEnumContractMapper.ToPartyType(code).Should().Be(expected);
    }

    [Fact]
    public void ToPartyType_Should_NotTrimOrAcceptDisplayFallback()
    {
        ApiEnumContractMapper.ToPartyType(" TENANT ").Should().BeNull();
        ApiEnumContractMapper.ToPartyType("Người thuê").Should().BeNull();
    }

    [Theory]
    [InlineData("MALE", GenderEnum.Male)]
    [InlineData("FEMALE", GenderEnum.Female)]
    public void ToGender_Should_MapCanonicalCodes(string code, GenderEnum expected)
    {
        ApiEnumContractMapper.ToGender(code).Should().Be(expected);
    }

    [Theory]
    [InlineData("CCCD", IdentifierTypeEnum.Cccd)]
    [InlineData("PASSPORT", IdentifierTypeEnum.Passport)]
    public void ToIdentifierType_Should_MapCanonicalCodes(string code, IdentifierTypeEnum expected)
    {
        ApiEnumContractMapper.ToIdentifierType(code).Should().Be(expected);
    }

    [Theory]
    [InlineData("PENDING", KycStatusEnum.Pending)]
    [InlineData("APPROVED", KycStatusEnum.Approved)]
    [InlineData("REJECTED", KycStatusEnum.Rejected)]
    public void ToKycStatus_Should_MapCanonicalCodes(string code, KycStatusEnum expected)
    {
        ApiEnumContractMapper.ToKycStatus(code).Should().Be(expected);
    }

    [Fact]
    public void ToPartyTypes_Should_DropUnknownCodes_WithoutUsingDisplayNames()
    {
        var result = ApiEnumContractMapper.ToPartyTypes(["TENANT", "Chủ nhà", "LANDLORD"]);

        result.Should().Equal(PartyTypeEnum.Tenant, PartyTypeEnum.Landlord);
    }
}
