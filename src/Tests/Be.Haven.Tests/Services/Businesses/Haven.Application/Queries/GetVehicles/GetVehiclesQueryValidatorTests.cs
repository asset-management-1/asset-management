using Haven.Application.Queries.GetVehicles;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetVehicles;

public sealed class GetVehiclesQueryValidatorTests
{
    private readonly GetVehiclesQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_RoomIdentifierIsValid()
    {
        var result = _validator.Validate(new GetVehiclesQuery(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_RoomIdentifierIsEmpty()
    {
        var result = _validator.Validate(new GetVehiclesQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(GetVehiclesQuery.RoomId));
    }

    [Fact]
    public void GetVehiclesQuery_Should_ExposeOnlyRoomScope()
    {
        var propertyNames = typeof(GetVehiclesQuery)
            .GetProperties()
            .Select(property => property.Name);

        propertyNames.Should().BeEquivalentTo([nameof(GetVehiclesQuery.RoomId)]);
    }
}
