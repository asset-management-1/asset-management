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

    [Fact]
    public async Task CreateText_Should_PropagateTransaction_WhenTransactionIsProvided()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        // Act
        var result = DapperCommandOptionsHelper.CreateText(transaction: transaction);

        // Assert
        result.Transaction.Should().BeSameAs(transaction);
    }
}
