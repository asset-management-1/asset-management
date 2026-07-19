using Authentication.Api.Swagger.Examples;
using Authentication.Application.Dtos.Authentications.ExternalProviders;
using Authentication.Application.Dtos.Users.Sessions;
using AuthenticationMessages = Authentication.Application.Constants.ApplicationMessageConstants;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Api.Swagger;

public sealed class AuthenticationSwaggerExamplesTests
{
    [Theory]
    [InlineData(typeof(AuthenticationSwaggerExamples.RegisterResponse), AuthenticationMessages.OtpMessages.OTP_SENT_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.VerifyRegisterEmailResponse), AuthenticationMessages.AccountMessages.REGISTRATION_COMPLETED_SUCCESS_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.LogoutResponse), AuthenticationMessages.SessionMessages.LOGOUT_SUCCESS_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.ForgotPasswordResponse), AuthenticationMessages.OtpMessages.FORGOT_PASSWORD_SUCCESS_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.ChangeForgotPasswordResponse), AuthenticationMessages.PasswordMessages.PASSWORD_CHANGED_SUCCESS_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.UpdateUserInfoResponse), AuthenticationMessages.ProfileMessages.USER_INFO_UPDATED_SUCCESS_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.ChangeEmailResponse), AuthenticationMessages.ProfileMessages.CHANGE_EMAIL_OTP_SENT_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.VerifyChangeEmailOtpResponse), AuthenticationMessages.ProfileMessages.CHANGE_EMAIL_COMPLETED_SUCCESS_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.LinkExternalProviderResponse), AuthenticationMessages.ExternalProviderMessages.EXTERNAL_PROVIDER_LINKED_SUCCESS_MESSAGE)]
    [InlineData(typeof(AuthenticationSwaggerExamples.UnlinkExternalProviderResponse), AuthenticationMessages.ExternalProviderMessages.EXTERNAL_PROVIDER_UNLINKED_SUCCESS_MESSAGE)]
    public void OperationStatusProvider_Should_UseEndpointMessage(Type providerType, string expectedMessage)
    {
        // Arrange
        var provider = (ISwaggerExampleProvider)Activator.CreateInstance(providerType)!;

        // Act
        var result = (ResponseDto<OperationStatusResponseDto>)provider.GetExample();

        // Assert
        result.Data.IsSuccess.Should().BeTrue();
        result.Data.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void ExternalLoginResponse_Should_DocumentExistingAccountBranch()
    {
        // Arrange
        var provider = new AuthenticationSwaggerExamples.ExternalLoginResponse();

        // Act
        var result = (ResponseDto<ExternalLoginResponseDto>)provider.GetExample();

        // Assert
        result.Data.IsNewRegistration.Should().BeFalse();
        result.Data.Login.Should().NotBeNull();
        result.Data.Registration.Should().BeNull();
    }

    [Fact]
    public void ExternalRegistrationRequiredResponse_Should_DocumentPrefillBranch()
    {
        // Arrange
        var provider = new AuthenticationSwaggerExamples.ExternalRegistrationRequiredResponse();

        // Act
        var result = (ResponseDto<ExternalLoginResponseDto>)provider.GetExample();

        // Assert
        result.Data.IsNewRegistration.Should().BeTrue();
        result.Data.Login.Should().BeNull();
        result.Data.Registration.Should().NotBeNull();
        result.Data.Registration.Email.Should().Be("tenant@example.com");
        result.Data.Registration.FullName.Should().Be("Nguyen Van A");
    }
}
