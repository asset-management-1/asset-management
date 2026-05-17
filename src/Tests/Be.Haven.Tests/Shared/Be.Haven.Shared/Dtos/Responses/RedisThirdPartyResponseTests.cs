namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Responses;

public sealed class RedisThirdPartyResponseTests
{
    [Fact]
    public void DeserializeObject_Should_MapJsonPropertyNames_When_ThirdPartyResponseUsesContractNames()
    {
        // Arrange
        const string json = """
                            {
                              "status": "OK",
                              "isSuccess": true,
                              "totalCount": 2,
                              "message": "done",
                              "errors": ["none"]
                            }
                            """;

        // Act
        var result = JsonConvert.DeserializeObject<RedisThirdPartyResponse>(json);

        // Assert
        result.Status.Should().Be("OK");
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(2);
        result.Message.Should().Be("done");
        result.Errors.Should().ContainSingle().Which.Should().Be("none");
    }
}
