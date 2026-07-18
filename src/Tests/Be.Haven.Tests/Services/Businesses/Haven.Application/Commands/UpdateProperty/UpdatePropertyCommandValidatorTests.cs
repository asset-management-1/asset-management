using Haven.Application.Commands.UpdateProperty;
using Haven.Application.Dtos.Properties.Update;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.UpdateProperty;

public sealed class UpdatePropertyCommandValidatorTests
{
    private readonly UpdatePropertyCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_UpdatePayloadIsValid()
    {
        var command = BuildValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_SharedBedRoomHasNoTotalBeds()
    {
        var command = BuildValidCommand();
        command.Structure.Floors[0].Rooms =
        [
            new UpdatePropertyRoomRequestDto
            {
                Name = "Phòng 205",
                TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
                RentalModeCode = MASTER_CODE_RENTAL_MODE_SHARED_BED,
                BaseRentAmount = 4_800_000
            }
        ];

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_NonSharedBedRoomSuppliesTotalBeds()
    {
        var command = BuildValidCommand();
        var room = BuildWholeRoom("101");
        room.TotalBeds = 1;
        command.Structure.Floors[0].Rooms =
        [
            room
        ];

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Enforce_ParkingVehicleTypeRules()
    {
        var missingVehicleType = BuildValidCommand();
        missingVehicleType.ChargePolicies =
        [
            new UpdatePropertyChargePolicyRequestDto
            {
                Code = MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                Name = "Phí gửi xe máy",
                Amount = 100_000,
                CalculationMethodCode = CALCULATION_METHOD_FIXED
            }
        ];

        var nonParkingWithVehicleType = BuildValidCommand();
        nonParkingWithVehicleType.ChargePolicies =
        [
            new UpdatePropertyChargePolicyRequestDto
            {
                Code = "ELECTRIC",
                Name = "Tiền điện",
                Amount = 3_500,
                CalculationMethodCode = CALCULATION_METHOD_METER_READING,
                VehicleTypeCode = "MOTORBIKE"
            }
        ];

        _validator.Validate(missingVehicleType).IsValid.Should().BeFalse();
        _validator.Validate(nonParkingWithVehicleType).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_ChargePolicyHasNoCalculationMethod()
    {
        var command = BuildValidCommand();
        command.ChargePolicies =
        [
            new UpdatePropertyChargePolicyRequestDto
            {
                Code = "ELECTRIC",
                Name = "Tiền điện",
                Amount = 3_500
            }
        ];

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(UpdatePropertyChargePolicyRequestDto.CalculationMethodCode)));
    }

    [Fact]
    public void Validate_Should_Pass_When_ExistingNestedRowsSubmitOnlyChangedFields()
    {
        var command = new UpdatePropertyCommand
        {
            Id = Guid.NewGuid(),
            ChargePolicies =
            [
                new UpdatePropertyChargePolicyRequestDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Tiền điện mới"
                }
            ],
            Packages =
            [
                new UpdatePropertyPackageRequestDto
                {
                    Id = Guid.NewGuid(),
                    PriceAdjustment = 900_000
                }
            ]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_NewNestedRowsDoNotSupplyRequiredFields()
    {
        var command = new UpdatePropertyCommand
        {
            Id = Guid.NewGuid(),
            ChargePolicies = [new UpdatePropertyChargePolicyRequestDto { Name = "Tiền điện" }],
            Packages = [new UpdatePropertyPackageRequestDto { PriceAdjustment = 500_000 }]
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(UpdatePropertyChargePolicyRequestDto.Code)));
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(UpdatePropertyChargePolicyRequestDto.Amount)));
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(UpdatePropertyPackageRequestDto.Name)));
    }

    private static UpdatePropertyCommand BuildValidCommand()
    {
        return new UpdatePropertyCommand
        {
            Id = Guid.NewGuid(),
            Name = "Atlas Plaza",
            PropertyTypeCode = "MINI_APARTMENT",
            ProvinceCode = "HCM",
            DistrictCode = "Q1",
            WardCode = "P_BEN_NGHE",
            StreetAddress = "122 Đường Sáng Tạo",
            FormattedAddress = "122 Đường Sáng Tạo, Phường Bến Nghé, Quận 1, TP.HCM",
            Latitude = 10.776889m,
            Longitude = 106.700806m,
            Structure = new UpdatePropertyStructureRequestDto
            {
                Floors =
                [
                    new UpdatePropertyFloorRequestDto
                    {
                        FloorNumber = 1,
                        Rooms = [BuildWholeRoom("101")]
                    }
                ]
            },
            ChargePolicies =
            [
                new UpdatePropertyChargePolicyRequestDto
                {
                    Code = "ELECTRIC",
                    Name = "Tiền điện",
                    Amount = 3_500,
                    CalculationMethodCode = CALCULATION_METHOD_METER_READING
                },
                new UpdatePropertyChargePolicyRequestDto
                {
                    Code = MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                    Name = "Phí gửi xe máy",
                    Amount = 100_000,
                    CalculationMethodCode = CALCULATION_METHOD_FIXED,
                    VehicleTypeCode = "MOTORBIKE"
                }
            ],
            Packages =
            [
                new UpdatePropertyPackageRequestDto
                {
                    Name = "Gói Full Nội thất",
                    PriceAdjustment = 1_500_000,
                    Items =
                    [
                        new UpdatePropertyPackageItemRequestDto { Name = "Giường ngủ King Size" }
                    ]
                }
            ]
        };
    }

    private static UpdatePropertyRoomRequestDto BuildWholeRoom(string code)
    {
        return new UpdatePropertyRoomRequestDto
        {
            Name = $"Phòng {code}",
            TypeCode = MASTER_CODE_UNIT_TYPE_ROOM,
            RentalModeCode = MASTER_CODE_RENTAL_MODE_WHOLE_UNIT,
            AreaSqm = 28,
            BaseRentAmount = 5_500_000,
            DefaultDepositAmount = 5_500_000,
            IsPetAllowed = true
        };
    }
}
