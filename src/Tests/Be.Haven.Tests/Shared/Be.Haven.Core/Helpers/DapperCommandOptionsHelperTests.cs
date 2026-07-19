using Microsoft.EntityFrameworkCore.Storage;

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

    [Fact]
    public async Task CreateTransactionalText_Should_UseActiveEfTransactionAndCommandSettings()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DbContext>()
                      .UseSqlite(connection)
                      .Options;
        await using var dbContext = new DbContext(options);
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        var result = DapperCommandOptionsHelper.CreateTransactionalText(
            dbContext,
            cancellationTokenSource.Token);

        // Assert
        result.CommandType.Should().Be(CommandType.Text);
        result.CancellationToken.Should().Be(cancellationTokenSource.Token);
        result.Transaction.Should().BeSameAs(transaction.GetDbTransaction());
    }

    [Fact]
    public async Task CreateTransactionalText_Should_Throw_WhenEfTransactionIsNotActive()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DbContext>()
                      .UseSqlite(connection)
                      .Options;
        await using var dbContext = new DbContext(options);

        // Act
        var act = () => DapperCommandOptionsHelper.CreateTransactionalText(
            dbContext,
            CancellationToken.None);

        // Assert
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage(ACTIVE_DATABASE_TRANSACTION_REQUIRED);
    }
}
