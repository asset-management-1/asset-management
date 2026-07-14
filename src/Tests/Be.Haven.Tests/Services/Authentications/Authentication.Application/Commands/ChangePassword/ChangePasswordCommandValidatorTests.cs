using Authentication.Application.Commands.ChangePassword;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_NewPasswordContainsRequiredCharacterGroups()
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "Old@123a",
            NewPassword = "New@123a"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("new@123a")]
    [InlineData("NEW@1234")]
    [InlineData("New@abcd")]
    [InlineData("New12345")]
    public void Validate_Should_Fail_When_NewPasswordMissesRequiredCharacterGroup(string newPassword)
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = "Old@123a",
            NewPassword = newPassword
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }
}
