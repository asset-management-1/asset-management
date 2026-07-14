using Haven.Application.Commands.UpdateRoomPackage;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.UpdateRoomPackage;

public sealed class UpdateRoomPackageCommandValidatorTests
{
    private readonly UpdateRoomPackageCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_OnlyPackageIdentityIsSubmitted()
    {
        var command = new UpdateRoomPackageCommand
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid()
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_ItemsAreExplicitlyCleared()
    {
        var command = new UpdateRoomPackageCommand
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            Items = []
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_SubmittedPriceIsNegative()
    {
        var command = new UpdateRoomPackageCommand
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            PriceAdjustment = -1
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateRoomPackageCommand.PriceAdjustment));
    }
}
