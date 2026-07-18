using Haven.Application.Queries.GetTenantJoinPreview;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Queries.GetTenantJoinPreview;

public sealed class GetTenantJoinPreviewQueryValidatorTests
{
    private readonly GetTenantJoinPreviewQueryValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_TokenIsProvided()
    {
        var query = new GetTenantJoinPreviewQuery("abc123");

        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_TokenIsMissingOrTooLong()
    {
        var missing = new GetTenantJoinPreviewQuery(string.Empty);
        var tooLong = new GetTenantJoinPreviewQuery(new string('a', TENANT_JOIN_TOKEN_MAX_LENGTH + 1));

        _validator.Validate(missing).IsValid.Should().BeFalse();
        _validator.Validate(tooLong).IsValid.Should().BeFalse();
    }
}
