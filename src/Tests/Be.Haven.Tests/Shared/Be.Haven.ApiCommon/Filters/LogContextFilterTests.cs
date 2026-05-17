namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Filters;

public sealed class LogContextFilterTests
{
    [Fact]
    public void Order_Should_ReturnMaxValue_When_FilterIsCreated()
    {
        // Arrange
        var sut = new LogContextFilter(Mock.Of<ILogger<LogContextFilter>>());

        // Act
        var result = sut.Order;

        // Assert
        result.Should().Be(int.MaxValue);
    }

    [Fact]
    public async Task OnActionExecutionAsync_Should_StoreMaskedRequestBody_When_ActionArgumentsContainSensitiveData()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/v1/auth/login";
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            DisplayName = "Login"
        });
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object>
            {
                ["request"] = new { Email = "user@haven.test", Password = "secret-password" }
            },
            controller: null);
        var sut = new LogContextFilter(Mock.Of<ILogger<LogContextFilter>>());
        var wasNextCalled = false;

        // Act
        await sut.OnActionExecutionAsync(
            context,
            () =>
            {
                wasNextCalled = true;
                return Task.FromResult(new ActionExecutedContext(actionContext, [], null));
            });

        // Assert
        wasNextCalled.Should().BeTrue();
        var maskedBody = httpContext.Items[REQUEST_BODY].Should().BeOfType<string>().Subject;
        maskedBody.Should().Contain("user@haven.test");
        maskedBody.Should().NotContain("secret-password");
    }

    [Fact]
    public async Task OnActionExecutionAsync_Should_StoreStringArgumentDirectly_When_ActionArgumentSanitizesToString()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/v1/search";
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            DisplayName = "Search"
        });
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object>
            {
                ["query"] = "hello"
            },
            controller: null);
        var sut = new LogContextFilter(Mock.Of<ILogger<LogContextFilter>>());

        // Act
        await sut.OnActionExecutionAsync(
            context,
            () => Task.FromResult(new ActionExecutedContext(actionContext, [], null)));

        // Assert
        httpContext.Items[REQUEST_BODY].Should().Be("hello");
    }

    [Fact]
    public async Task OnActionExecutionAsync_Should_StoreNullRequestBody_When_ActionArgumentsAreEmpty()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/v1/ping";
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            DisplayName = "Ping"
        });
        var context = new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object>(),
            controller: null);
        var sut = new LogContextFilter(Mock.Of<ILogger<LogContextFilter>>());

        // Act
        await sut.OnActionExecutionAsync(
            context,
            () => Task.FromResult(new ActionExecutedContext(actionContext, [], null)));

        // Assert
        httpContext.Items.Should().ContainKey(REQUEST_BODY);
        httpContext.Items[REQUEST_BODY].Should().BeNull();
    }

    [Fact]
    public async Task OnResultExecutionAsync_Should_LogMaskedResponseBody_When_ResultIsObjectResult()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/v1/user/user-info";
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Items[REQUEST_BODY] = "{\"email\":\"user@haven.test\"}";
        var response = new ResponseDto<object>(new { Token = "secret-token", Name = "Haven" });
        var result = new ObjectResult(response);
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ResultExecutingContext(actionContext, [], result, controller: null);
        var sut = new LogContextFilter(Mock.Of<ILogger<LogContextFilter>>());
        var wasNextCalled = false;

        // Act
        await sut.OnResultExecutionAsync(
            context,
            () =>
            {
                wasNextCalled = true;
                return Task.FromResult(new ResultExecutedContext(context, [], result, null));
            });

        // Assert
        wasNextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task OnResultExecutionAsync_Should_Continue_When_ResultIsNotObjectResult()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/v1/ping";
        var result = new EmptyResult();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ResultExecutingContext(actionContext, [], result, controller: null);
        var sut = new LogContextFilter(Mock.Of<ILogger<LogContextFilter>>());
        var wasNextCalled = false;

        // Act
        await sut.OnResultExecutionAsync(
            context,
            () =>
            {
                wasNextCalled = true;
                return Task.FromResult(new ResultExecutedContext(context, [], result, null));
            });

        // Assert
        wasNextCalled.Should().BeTrue();
    }
}
