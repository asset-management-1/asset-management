namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class DapperCommandOptionsHelperTests
{
    [Fact]
    public void CreateText_Should_ReturnTextCommandOptions_WithCancellationToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var result = DapperCommandOptionsHelper.CreateText(cancellationTokenSource.Token);

        result.CommandType.Should().Be(CommandType.Text);
        result.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public void CreateText_Should_ReturnTextCommandOptions_WithDefaultCancellationToken()
    {
        var result = DapperCommandOptionsHelper.CreateText();

        result.CommandType.Should().Be(CommandType.Text);
        result.CancellationToken.CanBeCanceled.Should().BeFalse();
    }
}
