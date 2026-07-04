namespace Haven.Application.Mappings;

/// <summary>
/// Maps property read rows into nested response graphs.
/// </summary>
public static class PropertyResponseMapper
{
    /// <summary>
    /// Maps property rows into list response DTOs.
    /// </summary>
    /// <param name="properties">The property rows.</param>
    /// <param name="floors">The floor rows.</param>
    /// <param name="rooms">The room rows.</param>
    /// <param name="roomTenants">The active tenant rows.</param>
    /// <returns>The property list response items.</returns>
    public static IReadOnlyList<PropertyListItemResponseDto> MapList(
        IReadOnlyList<PropertyRowModel> properties,
        IReadOnlyList<PropertyFloorRowModel> floors,
        IReadOnlyList<PropertyRoomRowModel> rooms,
        IReadOnlyList<PropertyRoomTenantRowModel> roomTenants)
    {
        var floorLookup = BuildListFloorLookup(floors, rooms, roomTenants);

        return properties
            .Select(row =>
            {
                // Mapster handles the flat row projection; this mapper attaches derived floor groups.
                var item = row.Adapt<PropertyListItemResponseDto>();
                item.Floors = GetFloors(row.PropertyPublicId, floorLookup);
                return item;
            })
            .ToList();
    }

    /// <summary>
    /// Maps a property row and child rows into the detail response.
    /// </summary>
    /// <param name="row">The property row.</param>
    /// <param name="floors">The floor rows.</param>
    /// <param name="rooms">The room rows.</param>
    /// <param name="chargePolicies">The charge policy rows.</param>
    /// <param name="packageTemplates">The package template rows.</param>
    /// <param name="managementSummary">The management summary row.</param>
    /// <returns>The property detail response.</returns>
    public static PropertyDetailResponseDto MapDetail(
        PropertyRowModel row,
        IReadOnlyList<PropertyFloorRowModel> floors,
        IReadOnlyList<PropertyRoomRowModel> rooms,
        IReadOnlyList<PropertyChargePolicyRowModel> chargePolicies,
        IReadOnlyList<PropertyPackageTemplateRowModel> packageTemplates,
        PropertyManagementSummaryRowModel managementSummary)
    {
        var floorLookup = BuildDetailFloorLookup(floors, rooms);

        // Detail uses Mapster for flat fields and keeps nested UI sections explicit.
        var detail = row.Adapt<PropertyDetailResponseDto>();
        detail.Structure = row.Adapt<PropertyDetailStructureResponseDto>();
        detail.Structure.Floors = GetDetailFloors(row.PropertyPublicId, floorLookup);
        detail.ChargePolicies = chargePolicies.Adapt<List<PropertyChargePolicyResponseDto>>();
        detail.PackageTemplates = MapPackageTemplates(packageTemplates);
        detail.ManagementSummary = managementSummary.Adapt<PropertyManagementSummaryResponseDto>();

        return detail;
    }

    /// <summary>
    /// Maps generated units into created-property floor summaries.
    /// </summary>
    /// <param name="units">The generated unit entities.</param>
    /// <returns>The grouped created-floor response DTOs.</returns>
    public static IReadOnlyList<CreatedPropertyFloorResponseDto> MapCreatedFloors(IReadOnlyList<Unit> units)
    {
        // Response floors are derived from generated unit floor numbers; Haven does not persist a Floors table.
        return units
            .GroupBy(x => x.FloorNumber ?? 0)
            .OrderBy(x => x.Key)
            .Select(x => new CreatedPropertyFloorResponseDto
            {
                FloorNumber = x.Key,
                TotalRooms = x.Count(),
                RoomCodes = x.Select(unit => unit.UnitCode).OrderBy(code => code).ToList()
            })
            .ToList();
    }

    /// <summary>
    /// Maps the persisted creation graph into the compact create response.
    /// </summary>
    /// <param name="graph">The persisted creation graph.</param>
    /// <returns>The created-property response.</returns>
    public static CreatedPropertyResponseDto MapCreated(PropertyCreationGraphModel graph)
    {
        return new CreatedPropertyResponseDto
        {
            Id = graph.Property.PublicId,
            PropertyCode = graph.Property.PropertyCode,
            Name = graph.Property.Name,
            TotalFloors = graph.Property.TotalFloors ?? 0,
            TotalRooms = graph.Property.TotalUnits ?? graph.Units.Count,
            Floors = MapCreatedFloors(graph.Units)
        };
    }

    /// <summary>
    /// Builds the property-to-floor lookup for the landlord property list response.
    /// </summary>
    /// <param name="floors">The floor summary rows.</param>
    /// <param name="rooms">The room card rows.</param>
    /// <param name="roomTenants">The active tenant rows.</param>
    /// <returns>The grouped floor response lookup by property public identifier.</returns>
    private static IReadOnlyDictionary<Guid, IReadOnlyList<PropertyListFloorResponseDto>> BuildListFloorLookup(
        IReadOnlyList<PropertyFloorRowModel> floors,
        IReadOnlyList<PropertyRoomRowModel> rooms,
        IReadOnlyList<PropertyRoomTenantRowModel> roomTenants)
    {
        var tenantLookup = roomTenants
            .GroupBy(x => x.UnitPublicId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(tenant => tenant.Adapt<PropertyListRoomTenantResponseDto>()).ToList());
        var roomLookup = rooms
            .GroupBy(x => (x.PropertyPublicId, x.FloorNumber))
            .ToDictionary(
                x => x.Key,
                x => x.Select(room => MapListRoom(room, tenantLookup)).ToList());

        return floors
            .GroupBy(x => x.PropertyPublicId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<PropertyListFloorResponseDto>)x.Select(floor =>
                {
                    var response = floor.Adapt<PropertyListFloorResponseDto>();

                    // Floors are only room groups; even full-floor business cases still return the room cards.
                    response.Rooms = roomLookup.TryGetValue((floor.PropertyPublicId, floor.FloorNumber), out var floorRooms)
                        ? floorRooms
                        : [];
                    return response;
                }).ToList());
    }

    /// <summary>
    /// Maps one room row into its compact list card response.
    /// </summary>
    /// <param name="row">The room row.</param>
    /// <param name="tenantLookup">The tenant rows grouped by unit public identifier.</param>
    /// <returns>The room card response.</returns>
    private static PropertyListRoomResponseDto MapListRoom(
        PropertyRoomRowModel row,
        IReadOnlyDictionary<Guid, List<PropertyListRoomTenantResponseDto>> tenantLookup)
    {
        var response = row.Adapt<PropertyListRoomResponseDto>();

        // Room tenant data is always exposed through Tenants so the response shape stays consistent.
        if (tenantLookup.TryGetValue(row.UnitPublicId, out var tenants))
        {
            response.Tenants = tenants;
        }

        return response;
    }

    /// <summary>
    /// Gets list-floor responses for one property from the grouped lookup.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="floorLookup">The grouped floor lookup.</param>
    /// <returns>The property's floor responses, or an empty list.</returns>
    private static IReadOnlyList<PropertyListFloorResponseDto> GetFloors(
        Guid propertyPublicId,
        IReadOnlyDictionary<Guid, IReadOnlyList<PropertyListFloorResponseDto>> floorLookup)
    {
        return floorLookup.TryGetValue(propertyPublicId, out var floors)
            ? floors
            : [];
    }

    /// <summary>
    /// Builds the property-to-floor lookup for the property detail response.
    /// </summary>
    /// <param name="floors">The floor summary rows.</param>
    /// <param name="rooms">The room detail rows.</param>
    /// <returns>The grouped detail floor response lookup by property public identifier.</returns>
    private static IReadOnlyDictionary<Guid, IReadOnlyList<PropertyDetailFloorResponseDto>> BuildDetailFloorLookup(
        IReadOnlyList<PropertyFloorRowModel> floors,
        IReadOnlyList<PropertyRoomRowModel> rooms)
    {
        var roomLookup = rooms
            .GroupBy(x => (x.PropertyPublicId, x.FloorNumber))
            .ToDictionary(
                x => x.Key,
                x => x.Select(room => room.Adapt<PropertyDetailRoomResponseDto>()).ToList());

        return floors
            .GroupBy(x => x.PropertyPublicId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyList<PropertyDetailFloorResponseDto>)x.Select(floor =>
                {
                    var response = floor.Adapt<PropertyDetailFloorResponseDto>();
                    response.Rooms = roomLookup.TryGetValue((floor.PropertyPublicId, floor.FloorNumber), out var floorRooms)
                        ? floorRooms
                        : [];
                    return response;
                }).ToList());
    }

    /// <summary>
    /// Gets detail-floor responses for one property from the grouped lookup.
    /// </summary>
    /// <param name="propertyPublicId">The property public identifier.</param>
    /// <param name="floorLookup">The grouped detail floor lookup.</param>
    /// <returns>The property's detail floor responses, or an empty list.</returns>
    private static IReadOnlyList<PropertyDetailFloorResponseDto> GetDetailFloors(
        Guid propertyPublicId,
        IReadOnlyDictionary<Guid, IReadOnlyList<PropertyDetailFloorResponseDto>> floorLookup)
    {
        return floorLookup.TryGetValue(propertyPublicId, out var floors)
            ? floors
            : [];
    }

    /// <summary>
    /// Maps cloned unit-package rows into distinct package templates for the detail response.
    /// </summary>
    /// <param name="rows">The package template source rows.</param>
    /// <returns>The distinct package templates.</returns>
    private static IReadOnlyList<PropertyPackageTemplateResponseDto> MapPackageTemplates(
        IReadOnlyList<PropertyPackageTemplateRowModel> rows)
    {
        // Unit packages are cloned per room, so detail returns distinct package templates for the edit step.
        return rows
            .GroupBy(x => new { x.PackageCode, x.PackageName, x.PriceAdjustment })
            .OrderBy(x => x.Key.PackageCode)
            .Select(x => new PropertyPackageTemplateResponseDto
            {
                Code = x.Key.PackageCode,
                Name = x.Key.PackageName,
                PriceAdjustment = x.Key.PriceAdjustment,
                Items = x
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemName))
                    .OrderBy(item => item.DisplayOrder ?? int.MaxValue)
                    .ThenBy(item => item.ItemName)
                    .Select(item => new PropertyPackageTemplateItemResponseDto { Name = item.ItemName })
                    .DistinctBy(item => item.Name)
                    .ToList()
            })
            .ToList();
    }
}
