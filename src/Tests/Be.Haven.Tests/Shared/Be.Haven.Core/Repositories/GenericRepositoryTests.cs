namespace Be.Haven.Tests.Shared.Be.Haven.Core.Repositories;

public sealed class GenericRepositoryTests
{
    [Fact]
    public async Task AddAsync_Should_StageEntityForPersistence_When_EntityIsValid()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);
        var entity = new CoreRepositorySampleEntity
        {
            Name = "Alpha",
            Quantity = 3
        };

        // Act
        var result = await sut.AddAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        result.Should().BeSameAs(entity);
        context.Samples.Should().ContainSingle(x => x.Name == "Alpha" && x.Quantity == 3);
    }

    [Fact]
    public async Task AddRangeAsync_Should_StageAllEntitiesForPersistence_When_EntitiesAreProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        await sut.AddRangeAsync([
            new CoreRepositorySampleEntity { Name = "Alpha", Quantity = 1 },
            new CoreRepositorySampleEntity { Name = "Beta", Quantity = 2 }
        ]);
        await context.SaveChangesAsync();

        // Assert
        context.Samples.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_Should_AttachDetachedEntityAsModified_When_EntityIsDetached()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var entity = new CoreRepositorySampleEntity
        {
            Name = "Before",
            Quantity = 1
        };
        context.Samples.Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);
        var detached = new CoreRepositorySampleEntity
        {
            Id = entity.Id,
            Name = "After",
            Quantity = 9
        };

        // Act
        await sut.UpdateAsync(detached);
        await context.SaveChangesAsync();

        // Assert
        context.ChangeTracker.Clear();
        var updated = await context.Samples.SingleAsync(x => x.Id == entity.Id);
        updated.Name.Should().Be("After");
        updated.Quantity.Should().Be(9);
    }

    [Fact]
    public async Task UpdateRangeAsync_Should_AttachDetachedEntitiesAsModified_When_EntitiesAreDetached()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var ids = await context.Samples.OrderBy(x => x.Id).Select(x => x.Id).Take(2).ToListAsync();
        context.ChangeTracker.Clear();
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);
        var entities = new List<CoreRepositorySampleEntity>
        {
            new() { Id = ids[0], Name = "Updated Alpha", Quantity = 10 },
            new() { Id = ids[1], Name = "Updated Beta", Quantity = 20 }
        };

        // Act
        await sut.UpdateRangeAsync(entities);
        await context.SaveChangesAsync();

        // Assert
        context.ChangeTracker.Clear();
        var result = await context.Samples.OrderBy(x => x.Id).Take(2).ToListAsync();
        result.Select(x => x.Name).Should().Equal("Updated Alpha", "Updated Beta");
    }

    [Fact]
    public async Task DeleteAsync_Should_StageHardDelete_When_EntityIsRemovedWithoutUnitOfWork()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        var entity = new CoreRepositorySampleEntity
        {
            Name = "Remove",
            Quantity = 2
        };
        context.Samples.Add(entity);
        await context.SaveChangesAsync();
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        await sut.DeleteAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        context.Samples.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteRangeAsync_Should_StageHardDeleteForAllEntities_When_EntitiesAreProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var entities = await context.Samples.Where(x => x.Quantity <= 2).ToListAsync();
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        await sut.DeleteRangeAsync(entities);
        await context.SaveChangesAsync();

        // Assert
        context.Samples.Should().ContainSingle(x => x.Name == "Gamma");
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnEntity_When_KeyExists()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var id = await context.Samples.Where(x => x.Name == "Beta").Select(x => x.Id).SingleAsync();
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetByIdAsync(id);

        // Assert
        result.Name.Should().Be("Beta");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_Should_ReturnNoTrackingEntity_When_RowMatches()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        context.ChangeTracker.Clear();
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.FirstOrDefaultAsync(x => x.Name == "Beta");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Beta");
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task FirstOrDefaultAsync_Should_ProjectReadModel_When_SelectorIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.FirstOrDefaultAsync(
            x => x.Name == "Gamma",
            x => new CoreRepositorySampleReadModel
            {
                Name = x.Name,
                Quantity = x.Quantity
            });

        // Assert
        result.Name.Should().Be("Gamma");
        result.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task AnyAndCountAsync_Should_ApplyPredicate_When_PredicateIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var any = await sut.AnyAsync(x => x.Quantity > 2);
        var count = await sut.CountAsync(x => x.Quantity > 1);

        // Assert
        any.Should().BeTrue();
        count.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_Should_CountAllRows_When_PredicateIsMissing()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllRowsAsNoTrackingEntities_When_Called()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        context.ChangeTracker.Clear();
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnProjectedRows_When_SelectorIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetAllAsync(x => new CoreRepositorySampleReadModel
        {
            Name = x.Name,
            Quantity = x.Quantity
        });

        // Assert
        result.Should().HaveCount(3);
        result.Select(x => x.Name).Should().Equal("Alpha", "Beta", "Gamma");
    }

    [Fact]
    public async Task GetListAsync_Should_ReturnEntityRows_When_PredicateIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetListAsync(x => x.Quantity >= 2);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Equal("Beta", "Gamma");
    }

    [Fact]
    public async Task GetListAsync_Should_ReturnProjectedReadOnlyRows_When_SelectorIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetListAsync(
            x => x.Quantity >= 2,
            x => new CoreRepositorySampleReadModel
            {
                Name = x.Name,
                Quantity = x.Quantity
            });

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().Equal("Beta", "Gamma");
    }

    [Fact]
    public async Task GetPagedReponseAsync_Should_FilterSortAndPageRows_When_ParametersAreProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var request = new BaseParameterRequest(1, 2)
        {
            OrderBy = "Quantity desc",
            SearchTerm = "a"
        };
        request.SetSearchProps([new SearchFieldConfiguration(nameof(CoreRepositorySampleEntity.Name))]);
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetPagedReponseAsync(request);

        // Assert
        result.Total.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.Items.Select(x => x.Quantity).Should().Equal(3, 2);
    }

    [Fact]
    public async Task GetModelPagedReponseAsync_Should_ProjectRowsFromDbSet_When_ParameterIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var request = new BaseParameterRequest(1, 2)
        {
            OrderBy = "Quantity desc"
        };
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetModelPagedReponseAsync<BaseParameterRequest, CoreRepositorySampleReadModel>(request);

        // Assert
        result.Total.Should().Be(3);
        result.Items.Select(x => x.Name).Should().Equal("Gamma", "Beta");
    }

    [Fact]
    public async Task GetModelPagedReponseAsync_Should_ProjectRowsFromQueryable_When_QueryIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var request = new BaseParameterRequest(1, 2)
        {
            OrderBy = "Quantity desc"
        };
        var query = context.Samples.Select(x => new CoreRepositorySampleReadModel
        {
            Name = x.Name,
            Quantity = x.Quantity
        });
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetModelPagedReponseAsync<BaseParameterRequest, CoreRepositorySampleReadModel>(
            query,
            request);

        // Assert
        result.Total.Should().Be(3);
        result.Items.Select(x => x.Name).Should().Equal("Gamma", "Beta");
    }

    [Fact]
    public async Task GetModelPagedReponseAsync_Should_ProjectRowsToResponseType_When_QueryAndResponseTypeAreProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var request = new BaseParameterRequest(1, 2)
        {
            OrderBy = "Quantity desc"
        };
        var query = context.Samples.Select(x => new CoreRepositorySampleReadModel
        {
            Name = x.Name,
            Quantity = x.Quantity
        });
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetModelPagedReponseAsync<
            BaseParameterRequest,
            CoreRepositorySampleReadModel,
            CoreRepositorySampleReadModel>(
            query,
            request);

        // Assert
        result.Total.Should().Be(3);
        result.Items.Select(x => x.Name).Should().Equal("Gamma", "Beta");
    }

    [Fact]
    public async Task GetModelSingleQueryPagedReponseAsync_Should_ProjectRowsInSingleQuery_When_ParameterIsProvided()
    {
        // Arrange
        await using var connection = await OpenConnectionAsync();
        await using var context = CreateContext(connection);
        await SeedSamplesAsync(context);
        var request = new BaseParameterRequest(1, 2)
        {
            OrderBy = "Quantity desc"
        };
        var sut = new GenericRepository<CoreRepositorySampleEntity>(context);

        // Act
        var result = await sut.GetModelSingleQueryPagedReponseAsync<BaseParameterRequest, CoreRepositorySampleReadModel>(request);

        // Assert
        result.Total.Should().Be(3);
        result.Items.Select(x => x.Name).Should().Equal("Gamma", "Beta");
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

    private static async Task SeedSamplesAsync(CoreRepositoryDbContext context)
    {
        context.Samples.AddRange(
            new CoreRepositorySampleEntity
            {
                Name = "Alpha",
                Quantity = 1
            },
            new CoreRepositorySampleEntity
            {
                Name = "Beta",
                Quantity = 2
            },
            new CoreRepositorySampleEntity
            {
                Name = "Gamma",
                Quantity = 3
            });

        await context.SaveChangesAsync();
    }
}
