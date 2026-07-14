using Haven.Domain.Entities;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Repositories.UnitPackages;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Repositories.UnitPackages;

public sealed class UnitPackageItemRepositoryTests
{
    [Fact]
    public async Task ReplaceItemsAsync_Should_DeleteExistingItemsAndAddOrderedReplacementItems()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UnitPackageItemRepository(dbContext);
        var oldItem = new UnitPackageItem { ItemName = "Old item" };
        var unitPackage = new UnitPackage();
        unitPackage.Items.Add(oldItem);

        await repository.ReplaceItemsAsync(
            unitPackage,
            ["Giường ngủ", "Tủ lạnh"],
            CancellationToken.None);

        unitPackage.Items.Should().HaveCount(2);
        unitPackage.Items.Select(item => item.ItemName).Should().Equal("Giường ngủ", "Tủ lạnh");
        unitPackage.Items.Select(item => item.DisplayOrder).Should().Equal(1, 2);
        dbContext.Entry(oldItem).State.Should().Be(EntityState.Deleted);
        unitPackage.Items.Should().OnlyContain(item => dbContext.Entry(item).State == EntityState.Added);
    }

    [Fact]
    public async Task ReplaceItemsAsync_Should_DeleteExistingItemsAndSkipAdd_When_ReplacementIsEmpty()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UnitPackageItemRepository(dbContext);
        var oldItem = new UnitPackageItem { ItemName = "Old item" };
        var unitPackage = new UnitPackage();
        unitPackage.Items.Add(oldItem);

        await repository.ReplaceItemsAsync(
            unitPackage,
            [],
            CancellationToken.None);

        unitPackage.Items.Should().BeEmpty();
        dbContext.Entry(oldItem).State.Should().Be(EntityState.Deleted);
        dbContext.ChangeTracker
            .Entries<UnitPackageItem>()
            .Should()
            .OnlyContain(entry => entry.State == EntityState.Deleted);
    }

    private static HavenDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HavenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new HavenDbContext(options);
    }
}
