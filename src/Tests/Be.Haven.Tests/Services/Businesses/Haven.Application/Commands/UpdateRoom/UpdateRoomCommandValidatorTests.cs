using Haven.Application.Commands.UpdateRoom;
using Haven.Application.Dtos.Rooms.Update;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.UpdateRoom;

public sealed class UpdateRoomCommandValidatorTests
{
    private readonly UpdateRoomCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_UpdatePayloadIsValid()
    {
        var command = BuildValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_FloorNumberIsMissing()
    {
        var command = BuildValidCommand();
        command.FloorNumber = null;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_SharedBedRoomHasNoTotalBeds()
    {
        var command = BuildValidCommand();
        command.RentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED;
        command.TotalBeds = null;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateRoomCommand.TotalBeds));
    }

    [Fact]
    public void Validate_Should_Fail_When_NonSharedBedRoomSuppliesTotalBeds()
    {
        var command = BuildValidCommand();
        command.TotalBeds = 1;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateRoomCommand.TotalBeds));
    }

    [Fact]
    public void Validate_Should_Fail_When_OverrideModeIsUnsupported()
    {
        var command = BuildValidCommand();
        command.ChargePolicyMode = "UNKNOWN";

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(UpdateRoomCommand.ChargePolicyMode));
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomModesHaveNoReplacementRows()
    {
        var command = BuildValidCommand();
        command.ChargePolicyMode = ROOM_OVERRIDE_MODE_CUSTOM;
        command.PackageMode = ROOM_OVERRIDE_MODE_CUSTOM;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == global::Haven.Application.Constants.ApplicationErrorConstants.RoomErrors.ERROR_ROOM_CUSTOM_CHARGE_POLICIES_REQUIRED);
        result.Errors.Should().Contain(error => error.ErrorMessage == global::Haven.Application.Constants.ApplicationErrorConstants.RoomErrors.ERROR_ROOM_CUSTOM_PACKAGES_REQUIRED);
    }

    [Fact]
    public void Validate_Should_Pass_When_CustomModesHaveReplacementRows()
    {
        var command = BuildValidCommand();
        command.ChargePolicyMode = ROOM_OVERRIDE_MODE_CUSTOM;
        command.ChargePolicies =
        [
            new UpdateRoomChargePolicyRequestDto
            {
                Code = "ELECTRIC",
                Name = "Tiền điện",
                Amount = 3_500,
                CalculationMethodCode = CALCULATION_METHOD_METER_READING
            }
        ];
        command.PackageMode = ROOM_OVERRIDE_MODE_CUSTOM;
        command.Packages =
        [
            new UpdateRoomPackageRequestDto
            {
                Name = "Gói Cơ Bản",
                PriceAdjustment = 800_000,
                Items =
                [
                    new UpdateRoomPackageItemRequestDto { Name = "Giường ngủ" }
                ]
            }
        ];

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_ExistingCustomRowsSubmitOnlyChangedFields()
    {
        var command = new UpdateRoomCommand
        {
            Id = Guid.NewGuid(),
            ChargePolicyMode = ROOM_OVERRIDE_MODE_CUSTOM,
            ChargePolicies =
            [
                new UpdateRoomChargePolicyRequestDto
                {
                    Id = Guid.NewGuid(),
                    Amount = 4_000
                }
            ],
            PackageMode = ROOM_OVERRIDE_MODE_CUSTOM,
            Packages =
            [
                new UpdateRoomPackageRequestDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Gói mới"
                }
            ]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_NewCustomRowsDoNotSupplyRequiredFields()
    {
        var command = new UpdateRoomCommand
        {
            Id = Guid.NewGuid(),
            ChargePolicyMode = ROOM_OVERRIDE_MODE_CUSTOM,
            ChargePolicies = [new UpdateRoomChargePolicyRequestDto { Name = "Tiền điện" }],
            PackageMode = ROOM_OVERRIDE_MODE_CUSTOM,
            Packages = [new UpdateRoomPackageRequestDto { PriceAdjustment = 500_000 }]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(UpdateRoomChargePolicyRequestDto.Code)));
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(UpdateRoomChargePolicyRequestDto.Amount)));
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(UpdateRoomPackageRequestDto.Name)));
    }

    private static UpdateRoomCommand BuildValidCommand()
    {
        return new UpdateRoomCommand
        {
            Id = Guid.NewGuid(),
            FloorNumber = 1,
            Name = "Phòng 101",
            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
            AreaSqm = 28,
            BaseRentAmount = 5_500_000,
            DefaultDepositAmount = 5_500_000,
            IsPetAllowed = true,
            ChargePolicyMode = ROOM_OVERRIDE_MODE_COMMON,
            PackageMode = ROOM_OVERRIDE_MODE_COMMON
        };
    }
}
