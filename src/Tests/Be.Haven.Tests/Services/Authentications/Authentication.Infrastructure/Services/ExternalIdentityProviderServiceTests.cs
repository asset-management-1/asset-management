using Authentication.Infrastructure.Options.ExternalProviders;
using Authentication.Infrastructure.Services;

namespace Be.Haven.Tests.Services.Authentications.Authentication.Infrastructure.Services;

public sealed class ExternalIdentityProviderServiceTests
{
    [Fact]
    public async Task GetValidatedIdentityAsync_Should_ReturnFacebookProfile_WhenDebugAndProfileResponsesAreValid()
    {
        var handler = new QueueHttpMessageHandler(
            "{\"data\":{\"is_valid\":true,\"app_id\":\"app-id\",\"user_id\":\"facebook-user\",\"expires_at\":4102444800}}",
            "{\"id\":\"facebook-user\",\"name\":\"Nguyen Van A\",\"email\":\"user@example.com\"}");
        var sut = CreateSut(handler);

        var result = await sut.GetValidatedIdentityAsync("facebook", "user-access-token", CancellationToken.None);

        result.ProviderUserId.Should().Be("facebook-user");
        result.Email.Should().Be("user@example.com");
        result.CanAutoLinkByEmail.Should().BeTrue();
        handler.RequestUris.Should().HaveCount(2);
        handler.RequestUris[0].AbsolutePath.Should().Be("/v-test/debug_token");
        handler.RequestUris.Should().OnlyContain(uri => string.IsNullOrEmpty(uri.Query));
        handler.LoggableUrls.Should().NotContain(value =>
            value.Contains("user-access-token", StringComparison.Ordinal)
            || value.Contains("app-secret", StringComparison.Ordinal)
            || value.Contains("app-id|app-secret", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetValidatedIdentityAsync_Should_RejectFacebookToken_WhenAppIdDoesNotMatch()
    {
        var sut = CreateSut(new QueueHttpMessageHandler(
            "{\"data\":{\"is_valid\":true,\"app_id\":\"other-app\",\"user_id\":\"facebook-user\",\"expires_at\":4102444800}}"));

        Func<Task> act = () => sut.GetValidatedIdentityAsync("facebook", "user-access-token", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ApiException>();
        exception.Which.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetValidatedIdentityAsync_Should_ReturnProviderUnavailable_WhenFacebookProviderIsUnavailable(
        HttpStatusCode statusCode)
    {
        var sut = CreateSut(new QueueHttpMessageHandler(new ResponseSpec(statusCode, "{}")));

        Func<Task> act = () => sut.GetValidatedIdentityAsync("facebook", "user-access-token", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HttpStatusCodeException>();
        exception.Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task GetValidatedIdentityAsync_Should_ReturnProviderUnavailable_WhenFacebookPayloadIsMalformed()
    {
        var sut = CreateSut(new QueueHttpMessageHandler("not-json"));

        Func<Task> act = () => sut.GetValidatedIdentityAsync("facebook", "user-access-token", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HttpStatusCodeException>();
        exception.Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task GetValidatedIdentityAsync_Should_ReturnProviderUnavailable_WhenFacebookPayloadHasUnexpectedTypes()
    {
        var sut = CreateSut(new QueueHttpMessageHandler(
            "{\"data\":{\"is_valid\":\"yes\",\"app_id\":\"app-id\"}}"));

        Func<Task> act = () => sut.GetValidatedIdentityAsync("facebook", "user-access-token", CancellationToken.None);

        var exception = await act.Should().ThrowAsync<HttpStatusCodeException>();
        exception.Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    private static ExternalIdentityProviderService CreateSut(HttpMessageHandler handler)
    {
        var options = OptionsFactory.Create(new ExternalAuthenticationOptions
        {
            Providers =
            [
                new ExternalProviderOptions
                {
                    Name = "google",
                    MetadataAddress = "https://oidc.example/.well-known/openid-configuration",
                    Audiences = ["mobile-client"],
                    ValidIssuers = ["https://accounts.example"]
                },
                new ExternalProviderOptions
                {
                    Name = "facebook",
                    AppId = "app-id",
                    AppSecret = "app-secret",
                    GraphApiBaseUrl = "https://graph.example",
                    GraphApiVersion = "v-test"
                }
            ]
        });
        return new ExternalIdentityProviderService(
            options,
            CreateHttpClientFactory(handler),
            Mock.Of<ILogger<ExternalIdentityProviderService>>());
    }

    private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handler, false));
        return factory.Object;
    }

    private sealed record ResponseSpec(HttpStatusCode StatusCode, string Content);

    private sealed class QueueHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<ResponseSpec> _responses;

        public QueueHttpMessageHandler(params string[] responses)
            : this(responses.Select(content => new ResponseSpec(HttpStatusCode.OK, content)).ToArray())
        {
        }

        public QueueHttpMessageHandler(params ResponseSpec[] responses)
        {
            _responses = new Queue<ResponseSpec>(responses);
        }

        public List<Uri> RequestUris { get; } = [];
        public List<string> LoggableUrls { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri);
            LoggableUrls.Add(request.RequestUri.ToString());
            var response = _responses.Dequeue();
            return Task.FromResult(new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.Content, Encoding.UTF8, "application/json")
            });
        }
    }
}
