namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    private readonly Exception _exception;

    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    public FakeHttpMessageHandler(Exception exception)
    {
        _exception = exception;
    }

    public HttpRequestMessage LastRequest { get; private set; }

    public string LastContentText { get; private set; }

    public string LastContentType { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        LastRequest = request;

        if (request.Content is not null)
        {
            LastContentText = await request.Content.ReadAsStringAsync(cancellationToken);
            LastContentType = request.Content.Headers.ContentType?.MediaType;
        }

        if (_exception is not null)
        {
            throw _exception;
        }

        return _response;
    }
}
