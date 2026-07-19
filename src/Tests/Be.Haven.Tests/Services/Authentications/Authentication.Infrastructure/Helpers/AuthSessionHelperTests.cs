using System.IdentityModel.Tokens.Jwt;
using Authentication.Application.Mappings.Authentications;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Helpers;
using Authentication.Infrastructure.Mappings.Authentications;
using Authentication.Infrastructure.Models.Authentications.Sessions;
using Authentication.Infrastructure.Options.Authentications;
using Be.Haven.Core.Models.ClientDevices;
using static Be.Haven.Shared.Constants.Authentication.AuthConstants;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Helpers;

public sealed class AuthSessionHelperTests
{
    /// <summary>
    /// Ensures token issuance reuses EF navigation references without recursively cloning the loaded identity graph.
    /// </summary>
    [Fact]
    public void BuildIssueModel_Should_ReuseLoadedUserAndSession_WhenNavigationGraphIsBidirectional()
    {
        // Arrange
        var user = new User
        {
            Id = 10,
            PublicId = Guid.NewGuid(),
            UserName = "tenant@example.com",
            Email = "tenant@example.com"
        };
        var session = new RefreshToken
        {
            User = user,
            UserId = user.Id,
            SessionPublicId = Guid.NewGuid(),
            TokenHash = "current-hash"
        };
        user.RefreshTokens.Add(session);
        var request = new AuthSessionIssueRequestModel
        {
            User = user,
            RawRefreshToken = AuthSessionHelper.GenerateRefreshToken(),
            SessionRefreshToken = session,
            DeviceContext = new ClientDeviceContextModel { DeviceId = "device-1" },
            AuthOptions = new AuthOptions
            {
                Issuer = "Authentication",
                Audiences = ["Haven.Tests"],
                SecretKey = "0123456789ABCDEF0123456789ABCDEF"
            },
            JwtSecurityTokenHandler = new JwtSecurityTokenHandler()
        };

        // Act
        var result = AuthSessionHelper.BuildIssueModel(request);

        // Assert
        result.RefreshToken.Should().BeSameAs(session);
        result.LoginResponse.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void BuildIssueModel_Should_KeepDeviceMetadataInSession_AndExcludeItFromJwt()
    {
        var request = new AuthSessionIssueRequestModel
        {
            User = new User
            {
                Id = 10,
                PublicId = Guid.NewGuid(),
                UserName = "tenant@example.com",
                Email = "tenant@example.com"
            },
            RawRefreshToken = AuthSessionHelper.GenerateRefreshToken(),
            DeviceContext = new ClientDeviceContextModel
            {
                DeviceId = "device-1",
                DeviceName = "Chrome",
                DeviceType = "web",
                UserAgent = "test-agent",
                IpAddress = "127.0.0.1"
            },
            RenewSessionPublicId = true,
            AuthOptions = new AuthOptions
            {
                Issuer = "Authentication",
                Audiences = ["Haven.Tests"],
                SecretKey = "0123456789ABCDEF0123456789ABCDEF"
            },
            JwtSecurityTokenHandler = new JwtSecurityTokenHandler()
        };

        var result = AuthSessionHelper.BuildIssueModel(request);
        var jwt = request.JwtSecurityTokenHandler.ReadJwtToken(result.LoginResponse.AccessToken);

        result.RefreshToken.DeviceId.Should().Be("device-1");
        result.RefreshToken.DeviceName.Should().Be("Chrome");
        result.RefreshToken.DeviceType.Should().Be("web");
        result.RefreshToken.UserAgent.Should().Be("test-agent");
        result.RefreshToken.IpAddress.Should().Be("127.0.0.1");
        jwt.Claims.Should().Contain(claim => claim.Type == TokenClaimTypes.SESSION_ID);
        var claimTypes = jwt.Claims.Select(claim => claim.Type).ToArray();
        claimTypes.Should().NotContain("device_id");
        claimTypes.Should().NotContain("device_name");
        claimTypes.Should().NotContain("device_type");
        claimTypes.Should().NotContain("user_agent");
        claimTypes.Should().NotContain("ip_address");
    }
}
