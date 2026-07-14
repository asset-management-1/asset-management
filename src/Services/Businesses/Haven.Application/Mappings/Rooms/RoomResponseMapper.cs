namespace Haven.Application.Mappings.Rooms;

/// <summary>
/// Maps room read rows into nested response graphs.
/// </summary>
public static class RoomResponseMapper
{
    /// <summary>
    /// Maps flat room rows into property/floor grouped list response items.
    /// </summary>
    /// <param name="rooms">The room list rows.</param>
    /// <param name="tenants">The active tenant rows.</param>
    /// <returns>The grouped room list response.</returns>
    public static IReadOnlyList<RoomListPropertyResponseDto> MapList(
        IReadOnlyList<RoomListRoomRowModel> rooms,
        IReadOnlyList<RoomTenantRowModel> tenants)
    {
        // Build the tenant lookup once before reconstructing the property and floor hierarchy.
        var tenantLookup = tenants
            .GroupBy(x => x.UnitPublicId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(tenant => tenant.Adapt<RoomListTenantResponseDto>()).ToList());

        // The list query is room-shaped, so rebuild the property/floor graph expected by the UI.
        return rooms
            .GroupBy(room => new
            {
                room.PropertyPublicId,
                room.PropertyCode,
                room.PropertyName,
                room.PropertyAddress,
                room.PropertyTotalRooms,
                room.PropertyAvailableRooms
            })
            .Select(propertyGroup => new RoomListPropertyResponseDto
            {
                Id = propertyGroup.Key.PropertyPublicId,
                Code = propertyGroup.Key.PropertyCode,
                Name = propertyGroup.Key.PropertyName,
                Address = propertyGroup.Key.PropertyAddress,
                TotalRooms = propertyGroup.Key.PropertyTotalRooms,
                AvailableRooms = propertyGroup.Key.PropertyAvailableRooms,
                Floors = propertyGroup
                    .GroupBy(room => new
                    {
                        room.FloorNumber,
                        room.FloorTotalRooms,
                        room.FloorAvailableRooms
                    })
                    .OrderBy(floorGroup => floorGroup.Key.FloorNumber)
                    .Select(floorGroup => new RoomListFloorResponseDto
                    {
                        Number = floorGroup.Key.FloorNumber,
                        TotalRooms = floorGroup.Key.FloorTotalRooms,
                        AvailableRooms = floorGroup.Key.FloorAvailableRooms,
                        Rooms = floorGroup
                            .OrderBy(room => room.UnitCode)
                            .Select(room => MapListRoom(room, tenantLookup))
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();
    }

    /// <summary>
    /// Maps room detail rows into the room detail response.
    /// </summary>
    /// <param name="row">The room detail row.</param>
    /// <param name="chargePolicies">The effective charge policy rows.</param>
    /// <returns>The room detail response.</returns>
    public static RoomDetailResponseDto MapDetail(
        RoomDetailRowModel row,
        IReadOnlyList<RoomChargePolicyRowModel> chargePolicies)
    {
        // Room management collections are loaded through their own room-scoped APIs.
        return new RoomDetailResponseDto
        {
            BasicInfo = row.Adapt<RoomDetailBasicInfoResponseDto>(),
            ChargePolicies = chargePolicies.Adapt<List<RoomChargePolicyResponseDto>>()
        };
    }

    /// <summary>
    /// Maps one room row and attaches active tenant rows when present.
    /// </summary>
    /// <param name="row">The room list row.</param>
    /// <param name="tenantLookup">The active tenant rows grouped by room public identifier.</param>
    /// <returns>The room list response item.</returns>
    private static RoomListRoomResponseDto MapListRoom(
        RoomListRoomRowModel row,
        IReadOnlyDictionary<Guid, List<RoomListTenantResponseDto>> tenantLookup)
    {
        var response = row.Adapt<RoomListRoomResponseDto>();

        // Keep the room response shape stable when no active tenant rows exist.
        if (tenantLookup.TryGetValue(row.UnitPublicId, out var roomTenants))
        {
            response.Tenants = roomTenants;
        }

        return response;
    }

    /// <summary>
    /// Maps flat package item rows into distinct package templates.
    /// </summary>
    /// <param name="rows">The package and package-item rows.</param>
    /// <returns>The effective package templates for the room.</returns>
    public static IReadOnlyList<RoomPackageTemplateResponseDto> MapPackageTemplates(
        IReadOnlyList<RoomPackageTemplateRowModel> rows)
    {
        // Collapse repeated package/item rows into one ordered package template per public id.
        return rows
            .GroupBy(x => new { x.PackagePublicId, x.PackageName, x.PriceAdjustment })
            .OrderBy(x => x.Key.PackageName)
            .Select(x =>
            {
                var package = x.First().Adapt<RoomPackageTemplateResponseDto>();

                // Preserve configured display order while excluding blank item rows from left joins.
                package.Items = x
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemName))
                    .OrderBy(item => item.DisplayOrder ?? int.MaxValue)
                    .ThenBy(item => item.ItemName)
                    .Select(item => item.Adapt<RoomPackageTemplateItemResponseDto>())
                    .DistinctBy(item => item.Name)
                    .ToList();
                return package;
            })
            .ToList();
    }
}
