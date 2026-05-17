namespace Be.Haven.Tests.Shared.Be.Haven.Core.Models;

public sealed class BaseEntityTests
{
    [Fact]
    public void Properties_Should_RoundTripValues_When_Assigned()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 5, 17, 1, 2, 3, DateTimeKind.Utc);
        var createdBy = Guid.NewGuid();
        var updatedAt = createdAt.AddMinutes(5);
        var updatedBy = Guid.NewGuid();
        var sut = new SampleEntity();

        // Act
        sut.Id = 123;
        sut.PublicId = publicId;
        sut.CreatedAt = createdAt;
        sut.CreatedBy = createdBy;
        sut.UpdatedAt = updatedAt;
        sut.UpdatedBy = updatedBy;
        sut.IsDeleted = true;

        // Assert
        sut.Id.Should().Be(123);
        sut.PublicId.Should().Be(publicId);
        sut.CreatedAt.Should().Be(createdAt);
        sut.CreatedBy.Should().Be(createdBy);
        sut.UpdatedAt.Should().Be(updatedAt);
        sut.UpdatedBy.Should().Be(updatedBy);
        sut.IsDeleted.Should().BeTrue();
    }

    private sealed class SampleEntity : BaseEntity
    {
    }
}
