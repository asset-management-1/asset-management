using Authentication.Application.Commands.Register;
using Authentication.Domain.Enums;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Application.Commands.Register;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_PasswordContainsRequiredCharacterGroups()
    {
        var command = new RegisterCommand
        {
            UserName = "nguyenvana",
            PartyType = PartyTypeEnum.Tenant,
            Password = "Valid@123a",
            Email = "tenant@example.com",
            PhoneNumber = "0901234567",
            FullName = "Nguyen Van A"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("valid@123a")]
    [InlineData("VALID@123")]
    [InlineData("Valid@abc")]
    [InlineData("Valid123a")]
    public void Validate_Should_Fail_When_PasswordMissesRequiredCharacterGroup(string password)
    {
        var command = new RegisterCommand
        {
            UserName = "nguyenvana",
            PartyType = PartyTypeEnum.Tenant,
            Password = password,
            Email = "tenant@example.com",
            PhoneNumber = "0901234567",
            FullName = "Nguyen Van A"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterCommand.Password));
    }
}
