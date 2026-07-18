using Haven.Application.Commands.CreateProperty;
using Haven.Application.Dtos.Properties.Create;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateProperty;

public sealed class CreatePropertyCommandValidatorTests
{
    [Fact]
    public void Validate_Should_ReturnErrors_When_RequiredFieldsAndStructureAreInvalid()
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
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 0,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Name = string.Empty,
                                TypeCode = string.Empty,
                                RentalModeCode = string.Empty,
                                BaseRentAmount = -1,
                                DefaultDepositAmount = -1
                            }
                        ]
                    }
                ]
            },
            ChargePolicies =
            [
                new CreatePropertyChargePolicyRequestDto
                {
                    Amount = -1,
                    CalculationMethodCode = string.Empty
                }
            ]
        };

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.Name));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.PropertyTypeCode));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.Latitude));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyCommand.Longitude));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyFloorRequestDto.FloorNumber)));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyRoomRequestDto.Name)));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyRoomRequestDto.TypeCode)));
        result.Errors.Should().Contain(x => x.PropertyName.EndsWith(nameof(CreatePropertyRoomRequestDto.RentalModeCode)));
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
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
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
    public void Validate_Should_ReturnError_When_ParkingPolicyHasNoVehicleType()
    {
        var validator = new CreatePropertyChargePolicyRequestDtoValidator();
        var policy = new CreatePropertyChargePolicyRequestDto
        {
            Code = MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
            Amount = 100_000,
            CalculationMethodCode = CALCULATION_METHOD_FIXED
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
            CalculationMethodCode = CALCULATION_METHOD_METER_READING,
            VehicleTypeCode = "MOTORBIKE"
        };

        var result = validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyChargePolicyRequestDto.VehicleTypeCode));
    }

    [Fact]
    public void Validate_Should_ReturnError_When_ChargePolicyHasNoCalculationMethod()
    {
        var validator = new CreatePropertyChargePolicyRequestDtoValidator();
        var policy = new CreatePropertyChargePolicyRequestDto
        {
            Code = "ELECTRIC",
            Amount = 3500
        };

        var result = validator.Validate(policy);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreatePropertyChargePolicyRequestDto.CalculationMethodCode));
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
                Floors =
                [
                    new CreatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms =
                        [
                            new CreatePropertyRoomRequestDto
                            {
                                Name = "Phòng 101",
                                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                                RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT
                            }
                        ]
                    }
                ]
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
