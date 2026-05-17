namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class OperationStatusResponseHelperTests
{
    [Fact]
    public void Success_Should_CreateSuccessfulOperationStatusResponse_When_MessageIsProvided()
    {
        // Act
        var result = OperationStatusResponseHelper.Success("Done");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Done");
    }
}
