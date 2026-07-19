using Haven.Application.Dtos.Tenants.Join;
using Haven.Application.Mappings.Tenants;
using Haven.Application.Models.Tenants.Rows;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Mappings.Tenants;

public sealed class TenantResponseMappingTests
{
    [Fact]
    public void TenantJoinPreview_Should_MapPropertyAndRoom_When_RowIsProvided()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var source = new TenantJoinRoomRowModel
        {
            PropertyPublicId = propertyId,
            PropertyName = "Haven Nguyen Hue",
            RoomPublicId = roomId,
            RoomName = "101"
        };
        var config = new TypeAdapterConfig();
        new TenantResponseMapping().Register(config);

        // Act
        var result = source.Adapt<TenantJoinPreviewResponseDto>(config);

        // Assert
        result.Property.Id.Should().Be(propertyId);
        result.Property.Name.Should().Be("Haven Nguyen Hue");
        result.Room.Id.Should().Be(roomId);
        result.Room.Name.Should().Be("101");
    }
}
