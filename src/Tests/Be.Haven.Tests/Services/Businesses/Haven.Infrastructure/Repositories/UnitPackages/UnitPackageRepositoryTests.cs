using Haven.Domain.Entities;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Repositories.UnitPackages;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Repositories.UnitPackages;

public sealed class UnitPackageRepositoryTests
{
    [Fact]
    public void DeletePackagesWithItems_Should_SoftDeletePackagesAndLoadedItems()
    {
        using var dbContext = CreateDbContext();
        var repository = new UnitPackageRepository(dbContext);
        var firstItem = new UnitPackageItem { ItemName = "A" };
        var secondItem = new UnitPackageItem { ItemName = "B" };
        var firstPackage = new UnitPackage();
        var secondPackage = new UnitPackage();
        firstPackage.Items.Add(firstItem);
        secondPackage.Items.Add(secondItem);

        repository.DeletePackagesWithItems([firstPackage, secondPackage]);

        firstItem.IsDeleted.Should().BeTrue();
        secondItem.IsDeleted.Should().BeTrue();
        firstPackage.IsDeleted.Should().BeTrue();
        secondPackage.IsDeleted.Should().BeTrue();
    }

    private static HavenDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HavenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new HavenDbContext(options);
    }
}
