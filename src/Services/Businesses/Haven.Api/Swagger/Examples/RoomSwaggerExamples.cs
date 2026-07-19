namespace Haven.Api.Swagger.Examples;

/// <summary>
/// Groups room request, response, route, and query examples.
/// </summary>
internal static class RoomSwaggerExamples
{
    /// <summary>
    /// Provides room route and list-query values.
    /// </summary>
    internal sealed class Values : SwaggerExampleProvider<object>
    {
        /// <summary>
        /// Builds room route, filter, and paging examples.
        /// </summary>
        /// <returns>The room route and query values.</returns>
        protected override object BuildExample() => new { roomPublicId = HavenSwaggerExampleConstants.ROOM_ID, pageNumber = 1, pageSize = 20, searchText = "101" };
    }

    /// <summary>
    /// Provides an update-room request example.
    /// </summary>
    internal sealed class UpdateRequest : SwaggerExampleProvider<UpdateRoomCommand>
    {
        /// <summary>
        /// Builds an editable room request.
        /// </summary>
        /// <returns>The room update example.</returns>
        protected override UpdateRoomCommand BuildExample() => new()
        {
            Id = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID),
            FloorNumber = 1,
            Name = "101",
            TypeCode = "STUDIO",
            RentalModeCode = "WHOLE_ROOM",
            AreaSqm = 28.5m,
            BaseRentAmount = 4500000m,
            DefaultDepositAmount = 4500000m,
            IsPetAllowed = false
        };
    }

    /// <summary>
    /// Provides a grouped room list success envelope.
    /// </summary>
    internal sealed class ListResponse : SwaggerSuccessExampleProvider<PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>>>
    {
        /// <summary>
        /// Builds the grouped room list success envelope.
        /// </summary>
        /// <returns>The room list response.</returns>
        protected override PaginationResponse<IReadOnlyList<RoomListPropertyResponseDto>> BuildData() => new(
            [new RoomListPropertyResponseDto { Id = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID), Code = "PROPERTY-001", Name = "Haven Nguyen Hue", Address = "12 Nguyen Hue, Ho Chi Minh City", TotalRooms = 1, AvailableRooms = 1 }],
            1,
            20,
            1);
    }

    /// <summary>
    /// Provides a room detail success envelope.
    /// </summary>
    internal sealed class DetailResponse : SwaggerSuccessExampleProvider<RoomDetailResponseDto>
    {
        /// <summary>
        /// Builds the room detail success envelope.
        /// </summary>
        /// <returns>The room detail response.</returns>
        protected override RoomDetailResponseDto BuildData() => new()
        {
            BasicInfo = new RoomDetailBasicInfoResponseDto
            {
                Id = Guid.Parse(HavenSwaggerExampleConstants.ROOM_ID),
                Code = "ROOM-101",
                Name = "101",
                FloorNumber = 1,
                PropertyId = Guid.Parse(HavenSwaggerExampleConstants.PROPERTY_ID),
                PropertyCode = "PROPERTY-001",
                PropertyName = "Haven Nguyen Hue",
                TypeCode = "STUDIO",
                RentalModeCode = "WHOLE_ROOM",
                BaseRentAmount = 4500000m,
                DefaultDepositAmount = 4500000m
            }
        };
    }

    /// <summary>
    /// Provides a room operation success envelope.
    /// </summary>
    internal sealed class OperationResponse : SwaggerSuccessExampleProvider<OperationStatusResponseDto>
    {
        /// <summary>
        /// Builds a successful room mutation status envelope.
        /// </summary>
        /// <returns>The room operation response.</returns>
        protected override OperationStatusResponseDto BuildData() => new()
        {
            IsSuccess = true,
            Message = "Room operation completed successfully."
        };
    }
}
