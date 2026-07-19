using Authentication.Application.Dtos.Authentications.ExternalProviders;
using Authentication.Application.Dtos.Users.Sessions;
using Authentication.Application.Mappings.Authentications;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Mappings;

public sealed class AuthenticationDtoMappingTests
{
    [Fact]
    public void ExternalIdentityProfile_Should_MapRegistrationPrefillAndNormalizeEmail()
    {
        var config = new TypeAdapterConfig();
        new AuthenticationDtoMapping().Register(config);
        var profile = new ExternalIdentityProfileResponseDto
        {
            ProviderUserId = "provider-user",
            Email = " User@Example.COM ",
            FullName = "Nguyen Van A",
            EmailVerified = true
        };

        var result = profile.Adapt<ExternalRegistrationPrefillDto>(config);

        result.Email.Should().Be("user@example.com");
        result.FullName.Should().Be(profile.FullName);
    }

    [Fact]
    public void LoginResponse_Should_MapExistingExternalLoginEnvelope()
    {
        var config = new TypeAdapterConfig();
        new AuthenticationDtoMapping().Register(config);
        var login = new LoginResponseDto
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresIn = 900,
            TokenType = "Bearer"
        };

        var result = login.Adapt<ExternalLoginResponseDto>(config);

        result.IsNewRegistration.Should().BeFalse();
        result.Login.Should().BeEquivalentTo(login);
        result.Registration.Should().BeNull();
    }
}
