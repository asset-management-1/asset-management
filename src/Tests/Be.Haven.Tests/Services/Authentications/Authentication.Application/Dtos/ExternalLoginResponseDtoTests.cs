using Authentication.Application.Dtos.Authentications.ExternalProviders;
using Authentication.Application.Dtos.Users.Sessions;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Dtos;

public sealed class ExternalLoginResponseDtoTests
{
    [Fact]
    public void Serialize_Should_OmitRegistration_WhenExistingAccountLogsIn()
    {
        var model = new ExternalLoginResponseDto
        {
            IsNewRegistration = false,
            Login = new LoginResponseDto { AccessToken = "access", RefreshToken = "refresh" }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(
            model,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        json.Should().Contain("\"login\"");
        json.Should().NotContain("\"registration\"");
    }

    [Fact]
    public void Serialize_Should_OmitLogin_WhenRegistrationIsRequired()
    {
        var model = new ExternalLoginResponseDto
        {
            IsNewRegistration = true,
            Registration = new ExternalRegistrationPrefillDto
            {
                Email = "user@example.com",
                FullName = "User"
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(
            model,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));

        json.Should().Contain("\"registration\"");
        json.Should().NotContain("\"login\"");
    }
}
