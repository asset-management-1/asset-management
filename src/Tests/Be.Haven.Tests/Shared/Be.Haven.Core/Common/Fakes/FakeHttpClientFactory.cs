namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal sealed class FakeHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient;

    public FakeHttpClientFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string LastClientName { get; private set; }

    public HttpClient CreateClient(string name)
    {
        LastClientName = name;

        return _httpClient;
    }
}
