using Haven.Application.Commands.DeleteTenant;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.DeleteTenant;

public sealed class DeleteTenantCommandValidatorTests
{
    private readonly DeleteTenantCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_IdIsProvided()
    {
        var command = new DeleteTenantCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        var command = new DeleteTenantCommand(Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
