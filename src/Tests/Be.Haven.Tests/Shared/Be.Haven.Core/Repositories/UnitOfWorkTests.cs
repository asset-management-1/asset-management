namespace Be.Haven.Tests.Shared.Be.Haven.Core.Repositories;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_Should_StampCreatedAuditFields_When_EntityIsAdded()
    {
        // Arrange
        var userPublicId = Guid.Parse("fc07838a-daad-46b8-ba8b-eab01715dd65");
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var entity = new CoreAuditableSampleEntity
        {
            Name = "Created"
        };
        context.AuditableSamples.Add(entity);
        var sut = CreateSut(context, userPublicId);

        // Act
        var result = await sut.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        entity.CreatedBy.Should().Be(userPublicId);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.UpdatedBy.Should().BeNull();
        entity.UpdatedAt.Should().BeNull();
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync_Should_ConvertDeletedEntityToSoftDelete_When_EntityIsDeleted()
    {
        // Arrange
        var userPublicId = Guid.Parse("14a552b6-af23-4f42-82ec-3e3fc7f07d7a");
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var entity = new CoreAuditableSampleEntity
        {
            Name = "Soft delete"
        };
        context.AuditableSamples.Add(entity);
        await context.SaveChangesAsync();
        context.AuditableSamples.Remove(entity);
        var sut = CreateSut(context, userPublicId);

        // Act
        var result = await sut.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        context.ChangeTracker.Clear();
        var persisted = await context.AuditableSamples.SingleAsync(x => x.Id == entity.Id);
        persisted.IsDeleted.Should().BeTrue();
        persisted.UpdatedBy.Should().Be(userPublicId);
        persisted.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveChangesAsync_Should_StampUpdatedAuditFields_When_EntityIsModified()
    {
        // Arrange
        var userPublicId = Guid.Parse("5fb182c7-3a8f-4a19-b7d2-19ed71967fc5");
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var entity = new CoreAuditableSampleEntity
        {
            Name = "Original"
        };
        context.AuditableSamples.Add(entity);
        await context.SaveChangesAsync();
        entity.Name = "Updated";
        var sut = CreateSut(context, userPublicId);

        // Act
        var result = await sut.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        entity.UpdatedBy.Should().Be(userPublicId);
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Should_SaveAndCommit_When_ActionSucceeds()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = CreateSut(context, Guid.NewGuid());

        // Act
        await sut.ExecuteInTransactionAsync(_ =>
        {
            context.AuditableSamples.Add(new CoreAuditableSampleEntity
            {
                Name = "Committed"
            });

            return Task.CompletedTask;
        });

        // Assert
        context.AuditableSamples.Should().ContainSingle(x => x.Name == "Committed");
        sut.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Should_UseExistingTransaction_When_TransactionIsActive()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = CreateSut(context, Guid.NewGuid());
        await using var transaction = await sut.BeginTransactionAsync();

        // Act
        await sut.ExecuteInTransactionAsync(async _ =>
        {
            context.AuditableSamples.Add(new CoreAuditableSampleEntity
            {
                Name = "Outer transaction"
            });

            await context.SaveChangesAsync();
        });
        await transaction.CommitAsync();

        // Assert
        await using var verifyContext = CreateContext(connection);
        verifyContext.AuditableSamples.Should().ContainSingle(x => x.Name == "Outer transaction");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Should_RollBackAndWrapException_When_ActionFails()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = CreateSut(context, Guid.NewGuid());

        // Act
        var act = async () => await sut.ExecuteInTransactionAsync(_ =>
        {
            context.AuditableSamples.Add(new CoreAuditableSampleEntity
            {
                Name = "Rolled back"
            });

            throw new ApplicationException("boom");
        });

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.InnerException.Should().BeOfType<ApplicationException>();
        await using var verifyContext = CreateContext(connection);
        verifyContext.AuditableSamples.Should().BeEmpty();
        sut.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Should_ReturnResult_When_ActionSucceeds()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = CreateSut(context, Guid.NewGuid());

        // Act
        var result = await sut.ExecuteInTransactionAsync(_ =>
        {
            context.AuditableSamples.Add(new CoreAuditableSampleEntity
            {
                Name = "Result"
            });

            return Task.FromResult(42);
        });

        // Assert
        result.Should().Be(42);
        context.AuditableSamples.Should().ContainSingle(x => x.Name == "Result");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Should_ReturnResultWithinExistingTransaction_When_TransactionIsActive()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = CreateSut(context, Guid.NewGuid());
        await using var transaction = await sut.BeginTransactionAsync();

        // Act
        var result = await sut.ExecuteInTransactionAsync(_ => Task.FromResult("outer-result"));
        await transaction.RollbackAsync();

        // Assert
        result.Should().Be("outer-result");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Should_RollBackAndWrapException_When_ResultActionFails()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = CreateSut(context, Guid.NewGuid());

        // Act
        var act = async () => await sut.ExecuteInTransactionAsync<int>(_ =>
        {
            context.AuditableSamples.Add(new CoreAuditableSampleEntity
            {
                Name = "Failed result"
            });

            throw new ApplicationException("result failed");
        });

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.InnerException.Should().BeOfType<ApplicationException>();
        await using var verifyContext = CreateContext(connection);
        verifyContext.AuditableSamples.Should().BeEmpty();
    }

    private static UnitOfWork<CoreRepositoryDbContext> CreateSut(
        CoreRepositoryDbContext context,
        Guid userPublicId)
    {
        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.UserId()).Returns(userPublicId);

        return new UnitOfWork<CoreRepositoryDbContext>(
            context,
            authService.Object,
            Mock.Of<ILogger<UnitOfWork<CoreRepositoryDbContext>>>());
    }

    private static async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        return connection;
    }

    private static CoreRepositoryDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<CoreRepositoryDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new CoreRepositoryDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}
