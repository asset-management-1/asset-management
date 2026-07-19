namespace Haven.Api.Swagger.Examples;

/// <summary>
/// Groups room-package request, response, route, and query examples.
/// </summary>
internal static class RoomPackageSwaggerExamples
{
    /// <summary>
    /// Provides room-package route and query values.
    /// </summary>
    internal sealed class Values : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds package and room identifier examples.
        /// </summary>
        /// <returns>The room-package route and query values.</returns>
        protected override object BuildExample() => new { id = HavenSwaggerExampleConstants.PACKAGE_ID, roomId = HavenSwaggerExampleConstants.ROOM_ID };
    }

    /// <summary>
    /// Provides a create-room-package request example.
    /// </summary>
    internal sealed class CreateRequest : SwaggerExampleProvider<CreateRoomPackageCommand>
    {
        /// <summary>
        /// Builds a room-owned package creation request.
        /// </summary>
        /// <returns>The room-package creation example.</returns>
        protected override CreateRoomPackageCommand BuildExample() => new()
        {
            RoomId = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID),
            Name = "Furniture package",
            PriceAdjustment = 500000m,
            Items = [new RoomPackageItemRequestDto { Name = "Bed" }, new RoomPackageItemRequestDto { Name = "Wardrobe" }]
        };
    }

    /// <summary>
    /// Provides an update-room-package request example.
    /// </summary>
    internal sealed class UpdateRequest : SwaggerExampleProvider<UpdateRoomPackageCommand>
    {
        /// <summary>
        /// Builds an editable room-package request.
        /// </summary>
        /// <returns>The room-package update example.</returns>
        protected override UpdateRoomPackageCommand BuildExample() => new()
        {
            Id = Guid.Parse(HavenSwaggerExampleConstants.PACKAGE_ID),
            RoomId = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID),
            Name = "Premium furniture package",
            PriceAdjustment = 650000m,
            Items = [new RoomPackageItemRequestDto { Name = "Bed" }, new RoomPackageItemRequestDto { Name = "Wardrobe" }, new RoomPackageItemRequestDto { Name = "Desk" }]
        };
    }

    /// <summary>
    /// Provides a room-package list success envelope.
    /// </summary>
    internal sealed class ListResponse : SwaggerSuccessExampleProvider<RoomPackageListResponseDto>
    {
        /// <summary>
        /// Builds the effective room-package list success envelope.
        /// </summary>
        /// <returns>The room-package list response.</returns>
        protected override RoomPackageListResponseDto BuildData() => new()
        {
            Room = BuildRoom(),
            Packages = [BuildPackage()]
        };
    }

    /// <summary>
    /// Provides a room-package detail success envelope.
    /// </summary>
    internal sealed class DetailResponse : SwaggerSuccessExampleProvider<RoomPackageDetailResponseDto>
    {
        /// <summary>
        /// Builds the room-package detail success envelope.
        /// </summary>
        /// <returns>The room-package detail response.</returns>
        protected override RoomPackageDetailResponseDto BuildData() => new()
        {
            Room = BuildRoom(),
            Package = BuildPackage()
        };
    }

    /// <summary>
    /// Provides a room-package operation success envelope.
    /// </summary>
    internal sealed class OperationResponse : SwaggerSuccessExampleProvider<OperationStatusResponseDto>
    {
        /// <summary>
        /// Builds a successful room-package mutation status envelope.
        /// </summary>
        /// <returns>The room-package operation response.</returns>
        protected override OperationStatusResponseDto BuildData() => new()
        {
            IsSuccess = true,
            Message = "Room package deleted successfully."
        };
    }

    /// <summary>
    /// Builds the room context reused by room-package responses.
    /// </summary>
    /// <returns>The room context example.</returns>
    private static RoomPackageRoomResponseDto BuildRoom() => new()
    {
        Id = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID),
        Name = "101"
    };

    /// <summary>
    /// Builds the package payload reused by list and detail responses.
    /// </summary>
    /// <returns>The package example.</returns>
    private static RoomPackageTemplateResponseDto BuildPackage() => new()
    {
        Id = Guid.Parse(HavenSwaggerExampleConstants.PACKAGE_ID),
        Name = "Furniture package",
        PriceAdjustment = 500000m,
        Items = [new RoomPackageTemplateItemResponseDto { Name = "Bed" }, new RoomPackageTemplateItemResponseDto { Name = "Wardrobe" }]
    };
}
