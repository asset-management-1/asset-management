using Haven.Application.Commands.ConfirmTenantJoin;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.ConfirmTenantJoin;

public sealed class ConfirmTenantJoinCommandValidatorTests
{
    private readonly ConfirmTenantJoinCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_CommandIsValid()
    {
        var command = new ConfirmTenantJoinCommand
        {
            Token = "abc123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_TokenIsMissingOrTooLong()
    {
        var missing = new ConfirmTenantJoinCommand { Token = string.Empty };
        var tooLong = new ConfirmTenantJoinCommand { Token = new string('a', TENANT_JOIN_TOKEN_MAX_LENGTH + 1) };

        _validator.Validate(missing).IsValid.Should().BeFalse();
        _validator.Validate(tooLong).IsValid.Should().BeFalse();
    }
}
