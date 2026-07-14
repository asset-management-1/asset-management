using Haven.Application.Commands.CreateRoomPackage;
using Haven.Application.Dtos.RoomPackages.Common;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateRoomPackage;

public sealed class CreateRoomPackageCommandValidatorTests
{
    private readonly CreateRoomPackageCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_PackageFieldsAndUniqueItemsAreSubmitted()
    {
        var command = new CreateRoomPackageCommand
        {
            RoomId = Guid.NewGuid(),
            Name = "Gói Cao cấp",
            PriceAdjustment = 1_800_000,
            Items =
            [
                new RoomPackageItemRequestDto { Name = "Giường ngủ" },
                new RoomPackageItemRequestDto { Name = "Máy lạnh" }
            ]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_ItemNamesAreDuplicatedIgnoringCase()
    {
        var command = new CreateRoomPackageCommand
        {
            RoomId = Guid.NewGuid(),
            Name = "Gói Cao cấp",
            Items =
            [
                new RoomPackageItemRequestDto { Name = "Máy lạnh" },
                new RoomPackageItemRequestDto { Name = "MÁY LẠNH" }
            ]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateRoomPackageCommand.Items));
    }
}
