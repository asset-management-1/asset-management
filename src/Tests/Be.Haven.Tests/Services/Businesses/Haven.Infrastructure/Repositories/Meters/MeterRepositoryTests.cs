using Be.Haven.Core.Interfaces.Services;
using Haven.Domain.Entities;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Repositories.Meters;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Repositories.Meters;

public sealed class MeterRepositoryTests
{
    [Fact]
    public async Task GetEvidenceLinksForMutationAsync_Should_FilterEveryPolymorphicDiscriminator()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var meterId = 100L;
        var meterEntityTypeId = 200L;
        var meterPhotoLinkTypeId = 300L;
        var matchingLink = CreateDocumentLink(meterId, meterEntityTypeId, meterPhotoLinkTypeId);
        var wrongEntityTypeLink = CreateDocumentLink(meterId, meterEntityTypeId + 1, meterPhotoLinkTypeId);
        var wrongLinkTypeLink = CreateDocumentLink(meterId, meterEntityTypeId, 999L);
        dbContext.DocumentLinks.AddRange(matchingLink, wrongEntityTypeLink, wrongLinkTypeLink);
        await dbContext.SaveChangesAsync();

        var sut = new MeterRepository(dbContext, Mock.Of<IDapperService>());

        // Act
        var result = await sut.GetEvidenceLinksForMutationAsync(
            [meterId],
            meterEntityTypeId,
            meterPhotoLinkTypeId);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().BeSameAs(matchingLink);
    }

    private static DocumentLink CreateDocumentLink(long entityId, long entityTypeId, long linkTypeId)
    {
        return new DocumentLink
        {
            PublicId = Guid.NewGuid(),
            EntityId = entityId,
            EntityTypeId = entityTypeId,
            LinkTypeId = linkTypeId,
            StatusId = 1,
            Document = new Document
            {
                PublicId = Guid.NewGuid(),
                DocumentTypeId = 1,
                FileName = Guid.NewGuid().ToString(),
                OriginalFileName = "evidence.jpg",
                ContentType = JPEG_IMAGE_CONTENT_TYPE,
                FileExtension = JPG_IMAGE_FILE_EXTENSION,
                StorageProviderId = 1,
                StoragePath = Guid.NewGuid().ToString(),
                FileUrl = "https://example.test/evidence.jpg",
                Checksum = Guid.NewGuid().ToString(),
                StatusId = 1
            }
        };
    }

    private static HavenDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HavenDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new HavenDbContext(options);
    }
}
