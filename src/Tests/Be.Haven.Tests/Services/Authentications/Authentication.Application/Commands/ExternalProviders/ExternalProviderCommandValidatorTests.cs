using Authentication.Application.Commands.CompleteExternalRegistration;
using Authentication.Application.Commands.ExternalLink;
using Authentication.Application.Commands.ExternalLogin;
using Authentication.Application.Commands.ExternalUnlink;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.ExternalProviders;

public sealed class ExternalProviderCommandValidatorTests
{
    [Theory]
    [InlineData("google")]
    [InlineData("facebook")]
    public void ExternalLogin_Should_AcceptSupportedProvider(string provider)
    {
        var result = new ExternalLoginCommandValidator().Validate(new ExternalLoginCommand
        {
            Provider = provider,
            ExternalToken = "provider-token"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ExternalLogin_Should_RejectUnsupportedProvider()
    {
        var result = new ExternalLoginCommandValidator().Validate(new ExternalLoginCommand
        {
            Provider = "apple",
            ExternalToken = "provider-token"
        });

        result.Errors.Should().Contain(error => error.PropertyName == nameof(ExternalLoginCommand.Provider));
    }

    [Theory]
    [InlineData("google")]
    [InlineData("facebook")]
    public void CompleteRegistration_Should_AcceptSupportedProvider(string provider)
    {
        var result = new CompleteExternalRegistrationCommandValidator().Validate(
            new CompleteExternalRegistrationCommand
            {
                Provider = provider,
                ExternalToken = "provider-token",
                FullName = "Nguyen Van A",
                PhoneNumber = "0900000000",
                PartyType = "TENANT"
            });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CompleteRegistration_Should_RejectUnsupportedProvider()
    {
        var command = new CompleteExternalRegistrationCommand
        {
            Provider = "apple",
            ExternalToken = "provider-token",
            FullName = "Nguyen Van A",
            PhoneNumber = "0900000000",
            PartyType = "TENANT"
        };

        var result = new CompleteExternalRegistrationCommandValidator().Validate(command);

        result.Errors.Should().Contain(error => error.PropertyName == nameof(command.Provider));
    }

    [Theory]
    [InlineData("google")]
    [InlineData("facebook")]
    public void LinkProvider_Should_AcceptSupportedProvider(string provider)
    {
        var result = new LinkExternalProviderCommandValidator().Validate(new LinkExternalProviderCommand
        {
            Provider = provider,
            ExternalToken = "provider-token"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LinkProvider_Should_RejectUnsupportedProvider()
    {
        var result = new LinkExternalProviderCommandValidator().Validate(new LinkExternalProviderCommand
        {
            Provider = "apple",
            ExternalToken = "provider-token"
        });

        result.Errors.Should().Contain(error => error.PropertyName == nameof(LinkExternalProviderCommand.Provider));
    }

    [Theory]
    [InlineData("google")]
    [InlineData("facebook")]
    public void UnlinkProvider_Should_AcceptSupportedProvider(string provider)
    {
        var result = new UnlinkExternalProviderCommandValidator().Validate(
            new UnlinkExternalProviderCommand { Provider = provider });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UnlinkProvider_Should_RejectUnsupportedProvider()
    {
        var result = new UnlinkExternalProviderCommandValidator().Validate(
            new UnlinkExternalProviderCommand { Provider = "apple" });

        result.Errors.Should().Contain(error => error.PropertyName == nameof(UnlinkExternalProviderCommand.Provider));
    }
}
