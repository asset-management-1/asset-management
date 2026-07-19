using Authentication.Application.Commands.CompleteExternalRegistration;
using Authentication.Application.Dtos.Authentications.ExternalProviders;
using Authentication.Application.Mappings.Authentications;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Mappings;

public sealed class AuthenticationRequestMappingTests
{
    [Fact]
    public void CompleteExternalRegistration_Should_MapFlatFieldsAndNormalizePartyType()
    {
        var config = new TypeAdapterConfig();
        new AuthenticationRequestMapping().Register(config);
        var command = new CompleteExternalRegistrationCommand
        {
            Provider = "google",
            ExternalToken = "masked-token",
            FullName = "Nguyen Van A",
            PhoneNumber = "0900000000",
            PartyType = " tenant "
        };

        var result = command.Adapt<CompleteExternalRegistrationRequestDto>(config);

        result.Provider.Should().Be(command.Provider);
        result.ExternalToken.Should().Be(command.ExternalToken);
        result.FullName.Should().Be(command.FullName);
        result.PhoneNumber.Should().Be(command.PhoneNumber);
        result.PartyType.Should().Be("TENANT");
    }
}
