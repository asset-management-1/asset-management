using Haven.Application.Commands.CreateProperty;
using Haven.Application.Dtos.Properties.Common;
using Haven.Application.Dtos.Properties.Create;
using Haven.Application.Dtos.Properties.Detail;
using Haven.Application.Dtos.Properties.List;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateProperty;

public sealed class CreatePropertyCommandValidatorTests
{
    [Fact]
    public void Validate_Should_ReturnErrors_When_RequiredFieldsAndGeneratedStructureAreInvalid()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = string.Empty,
            PropertyTypeCode = string.Empty,
            Latitude = 120,
            Longitude = 200,
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                TotalFloors = 0,
                RoomsPerFloor = MAX_GENERATED_ROOMS_PER_FLOOR + 1,
                DefaultUnitTypeCode = string.Empty,
                DefaultRentalModeCode = string.Empty,
                DefaultBaseRentAmount = -1,
                DefaultDepositAmount = -1
            },
            ChargePolicies =
            [
                new CreatePropertyChargePolicyRequestDto
                {
                    Code = string.Empty,
                    Amount = -1
                }
            ]
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.Name));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.PropertyTypeCode));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.Latitude));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.Longitude));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyStructureRequestDto.TotalFloors)));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyStructureRequestDto.RoomsPerFloor)));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyChargePolicyRequestDto.Amount)));
    }

    [Fact]
    public void Validate_Should_Pass_When_MinimalPropertyCreateRequestIsValid()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                TotalFloors = 3,
                RoomsPerFloor = 4,
                DefaultUnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                DefaultRentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Pass_When_ExplicitFloorRoomSetupIsValid()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Code = "101",
                                Name = "Phòng 101",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                                BaseRentAmount = 5_500_000
                            }
                        ]
                    }
                ]
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ExplicitSharedBedRoomHasNoTotalBeds()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Code = "101",
                                Name = "Phòng 101",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED
                            }
                        ]
                    }
                ]
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyRoomRequestDto.TotalBeds)));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ExplicitWholeRoomSendsTotalBeds()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Code = "101",
                                Name = "Phòng 101",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
                                TotalBeds = 1
                            }
                        ]
                    }
                ]
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyRoomRequestDto.TotalBeds)));
    }

    [Fact]
    public void Validate_Should_Pass_When_ExplicitSharedBedRoomHasTotalBeds()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Code = "205",
                                Name = "Phòng 205",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED,
                                TotalBeds = 4
                            }
                        ]
                    }
                ]
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_ReturnError_When_QuickSharedBedSetupHasNoDefaultTotalBeds()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                TotalFloors = 1,
                RoomsPerFloor = 2,
                DefaultUnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                DefaultRentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyStructureRequestDto.DefaultTotalBeds)));
    }

    [Fact]
    public void Validate_Should_Pass_When_QuickSharedBedSetupHasDefaultTotalBeds()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                TotalFloors = 1,
                RoomsPerFloor = 2,
                DefaultUnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                DefaultRentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED,
                DefaultTotalBeds = 4
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ExplicitRoomsHaveDuplicateCodes()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Code = "101",
                                Name = "Phòng 101",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT
                            },
                            new CreatePropertyRoomRequestDto
                            {
                                Code = " 101 ",
                                Name = "Phòng 101B",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT
                            }
                        ]
                    }
                ]
            }
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("unit codes", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ParkingPolicyHasNoVehicleType()
    {
        var validator = new CreatePropertyChargePolicyRequestDtoValidator();
        var policy = new CreatePropertyChargePolicyRequestDto
        {
            Code = MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
            Amount = 100_000
        };

        var result = validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyChargePolicyRequestDto.VehicleTypeCode));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_NonParkingPolicyHasVehicleType()
    {
        var validator = new CreatePropertyChargePolicyRequestDtoValidator();
        var policy = new CreatePropertyChargePolicyRequestDto
        {
            Code = "ELECTRIC",
            Amount = 3500,
            VehicleTypeCode = "MOTORBIKE"
        };

        var result = validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyChargePolicyRequestDto.VehicleTypeCode));
    }

    [Fact]
    public void Validate_Should_ReturnErrors_When_PackageSetupIsInvalid()
    {
        var validator = new CreatePropertyCommandValidator();
        var command = new CreatePropertyCommand
        {
            Name = "Tòa nhà A",
            PropertyTypeCode = "BUILDING",
            StructureSetup = new CreatePropertyStructureRequestDto
            {
                TotalFloors = 1,
                RoomsPerFloor = 1,
                DefaultUnitTypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                DefaultRentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT
            },
            Packages =
            [
                new CreatePropertyPackageRequestDto
                {
                    Name = string.Empty,
                    PriceAdjustment = -1,
                    Items =
                    [
                        new CreatePropertyPackageItemRequestDto { Name = string.Empty }
                    ]
                }
            ]
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyPackageRequestDto.Name)));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyPackageRequestDto.PriceAdjustment)));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyPackageItemRequestDto.Name)));
    }
}
