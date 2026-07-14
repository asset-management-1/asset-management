using Haven.Application.Queries.GetTenantDetail;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetTenantDetail;

public sealed class GetTenantDetailQueryValidatorTests
{
    private readonly GetTenantDetailQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_IdIsProvided()
    {
        var query = new GetTenantDetailQuery(Guid.NewGuid());

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdIsEmpty()
    {
        var query = new GetTenantDetailQuery(Guid.Empty);

        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }
}
