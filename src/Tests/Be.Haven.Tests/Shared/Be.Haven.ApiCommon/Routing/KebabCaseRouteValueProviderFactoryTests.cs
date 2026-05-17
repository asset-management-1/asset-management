namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Routing;

public sealed class KebabCaseRouteValueProviderFactoryTests
{
    [Fact]
    public async Task CreateValueProviderAsync_Should_AddCamelCaseRouteKey_When_KebabCaseKeyExists()
    {
        // Arrange
        var context = CreateContext();
        context.ActionContext.RouteData.Values["vehicle-public-id"] = "vehicle-123";
        var sut = new KebabCaseRouteValueProviderFactory();

        // Act
        await sut.CreateValueProviderAsync(context);

        // Assert
        context.ActionContext.RouteData.Values["vehiclePublicId"].Should().Be("vehicle-123");
    }

    [Fact]
    public async Task CreateValueProviderAsync_Should_NotOverrideExistingCamelCaseKey_When_BothKeysExist()
    {
        // Arrange
        var context = CreateContext();
        context.ActionContext.RouteData.Values["vehicle-public-id"] = "kebab-value";
        context.ActionContext.RouteData.Values["vehiclePublicId"] = "existing-value";
        var sut = new KebabCaseRouteValueProviderFactory();

        // Act
        await sut.CreateValueProviderAsync(context);

        // Assert
        context.ActionContext.RouteData.Values["vehiclePublicId"].Should().Be("existing-value");
    }

    [Theory]
    [InlineData("version")]
    [InlineData("controller")]
    [InlineData("action")]
    [InlineData("area")]
    public async Task CreateValueProviderAsync_Should_SkipReservedRouteKeys_When_RouteKeyIsReserved(string key)
    {
        // Arrange
        var context = CreateContext();
        context.ActionContext.RouteData.Values[key] = "reserved";
        var sut = new KebabCaseRouteValueProviderFactory();

        // Act
        await sut.CreateValueProviderAsync(context);

        // Assert
        context.ActionContext.RouteData.Values.Should().ContainKey(key);
        context.ActionContext.RouteData.Values.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateValueProviderAsync_Should_CompleteWithoutChanges_When_RouteValuesAreEmpty()
    {
        // Arrange
        var context = CreateContext();
        var sut = new KebabCaseRouteValueProviderFactory();

        // Act
        await sut.CreateValueProviderAsync(context);

        // Assert
        context.ActionContext.RouteData.Values.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateValueProviderAsync_Should_SkipRouteKeys_When_KeyIsBlankOrNotKebabCase()
    {
        // Arrange
        var context = CreateContext();
        context.ActionContext.RouteData.Values[string.Empty] = "blank";
        context.ActionContext.RouteData.Values["vehiclePublicId"] = "camel";
        var sut = new KebabCaseRouteValueProviderFactory();

        // Act
        await sut.CreateValueProviderAsync(context);

        // Assert
        context.ActionContext.RouteData.Values.Should().HaveCount(2);
        context.ActionContext.RouteData.Values["vehiclePublicId"].Should().Be("camel");
    }

    private static ValueProviderFactoryContext CreateContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ValueProviderFactoryContext(actionContext);
    }
}
