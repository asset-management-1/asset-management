using Haven.Application.Queries.GetTenants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetTenants;

public sealed class GetTenantsQueryValidatorTests
{
    private readonly GetTenantsQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_QueryFiltersAreValid()
    {
        var query = new GetTenantsQuery
        {
            PropertyId = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            Search = "Nguyen",
            RoleCode = TENANT_ROLE_PRIMARY,
            OccupancyStatusCode = MASTER_CODE_OCCUPANCY_STATUS_ACTIVE,
            PageNumber = 1,
            PageSize = 20
        };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_OptionalIdsAreEmpty()
    {
        var query = new GetTenantsQuery
        {
            PropertyId = Guid.Empty,
            RoomId = Guid.Empty
        };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(GetTenantsQuery.PropertyId));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(GetTenantsQuery.RoomId));
    }

    [Fact]
    public void Validate_Should_Fail_When_RoleCodeIsUnsupported()
    {
        var query = new GetTenantsQuery
        {
            RoleCode = "UNKNOWN"
        };

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == global::Haven.Application.Constants.ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ROLE_INVALID);
    }
}
