using Haven.Application.Queries.GetRooms;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetRooms;

public sealed class GetRoomsQueryValidatorTests
{
    private readonly GetRoomsQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_QueryFiltersAreValid()
    {
        var query = new GetRoomsQuery
        {
            PropertyId = Guid.NewGuid(),
            PropertySearch = "Atlas",
            Search = "101",
            FloorNumber = 1,
            RoomStatusCode = "AVAILABLE",
            RentalModeCode = "WHOLE_UNIT",
            PaymentStatusCode = "PAID",
            PageNumber = 1,
            PageSize = 20
        };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_PropertyIdIsEmpty()
    {
        var query = new GetRoomsQuery
        {
            PropertyId = Guid.Empty
        };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(GetRoomsQuery.PropertyId));
    }
}
