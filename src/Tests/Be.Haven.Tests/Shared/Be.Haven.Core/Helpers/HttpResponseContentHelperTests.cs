namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class HttpResponseContentHelperTests
{
    [Fact]
    public async Task ReadMaskedContentAsync_Should_ReturnDefaultText_When_ContentIsNull()
    {
        // Act
        var result = await HttpResponseContentHelper.ReadMaskedContentAsync(null);

        // Assert
        result.Should().Be(DEFAULT_TEXT);
    }

    [Fact]
    public async Task ReadMaskedContentAsync_Should_ReturnDefaultText_When_ContentIsBlank()
    {
        // Arrange
        using var content = new StringContent("   ");

        // Act
        var result = await HttpResponseContentHelper.ReadMaskedContentAsync(content);

        // Assert
        result.Should().Be(DEFAULT_TEXT);
    }

    [Fact]
    public async Task ReadMaskedContentAsync_Should_MaskSensitiveContent_When_ResponseBodyContainsSecrets()
    {
        // Arrange
        using var content = new StringContent("""{"refreshToken":"refresh-123","message":"failed"}""");

        // Act
        var result = await HttpResponseContentHelper.ReadMaskedContentAsync(content);

        // Assert
        result.Should().Contain("\"refreshToken\":\"******\"");
        result.Should().Contain("\"message\":\"failed\"");
        result.Should().NotContain("refresh-123");
    }

    [Fact]
    public async Task ReadMaskedContentAsync_Should_ReturnDefaultText_When_ContentReadFails()
    {
        // Arrange
        using var content = new ThrowingHttpContentStub(new InvalidOperationException("read failed"));

        // Act
        var result = await HttpResponseContentHelper.ReadMaskedContentAsync(content);

        // Assert
        result.Should().Be(DEFAULT_TEXT);
    }

    [Fact]
    public async Task ReadMaskedContentAsync_Should_PropagateOperationCanceledException_When_ContentReadIsCanceled()
    {
        // Arrange
        using var content = new ThrowingHttpContentStub(new OperationCanceledException());

        // Act
        var act = () => HttpResponseContentHelper.ReadMaskedContentAsync(content);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed class ThrowingHttpContentStub : HttpContent
    {
        private readonly Exception _exception;

        public ThrowingHttpContentStub(Exception exception)
        {
            _exception = exception;
        }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext context)
        {
            return Task.FromException(_exception);
        }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext context,
            CancellationToken cancellationToken)
        {
            return Task.FromException(_exception);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;

            return false;
        }
    }
}
