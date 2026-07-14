namespace Haven.Application.Mappings.Properties;

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
        // Group child rows once so mapping every property header does not repeatedly scan the same floor data.
        var floorLookup = BuildListFloorLookup(floors, rooms, roomTenants);

        // Map flat property headers, then attach the pre-grouped floor and room hierarchy required by the list screen.
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
    /// <param name="wholeBuildingRental">The whole-building representative unit row with optional contract data.</param>
    /// <returns>The property detail response.</returns>
    public static PropertyDetailResponseDto MapDetail(
        PropertyRowModel row,
        IReadOnlyList<PropertyFloorRowModel> floors,
        IReadOnlyList<PropertyRoomRowModel> rooms,
        IReadOnlyList<PropertyChargePolicyRowModel> chargePolicies,
        IReadOnlyList<PropertyPackageTemplateRowModel> packageTemplates,
        PropertyWholeBuildingRentalRowModel wholeBuildingRental)
    {
        // Build the room hierarchy before mapping the detail's independent display sections.
        var floorLookup = BuildDetailFloorLookup(floors, rooms);

        // Map property and structure headers from the read row while preserving explicit nested response ownership.
        var detail = row.Adapt<PropertyDetailResponseDto>();
        detail.Structure = row.Adapt<PropertyDetailStructureResponseDto>();
        detail.Structure.Floors = GetDetailFloors(row.PropertyPublicId, floorLookup);
        // Assemble independent detail sections after the structure hierarchy is attached.
        detail.WholeBuildingRental = MapWholeBuildingRental(wholeBuildingRental);
        detail.ChargePolicies = chargePolicies.Adapt<List<PropertyChargePolicyResponseDto>>();
        detail.PackageTemplates = MapPackageTemplates(packageTemplates);

        return detail;
    }

    /// <summary>
    /// Maps persisted units into created-property floor summaries.
    /// </summary>
    /// <param name="units">The created unit entities.</param>
    /// <returns>The grouped created-floor response DTOs.</returns>
    public static IReadOnlyList<CreatedPropertyFloorResponseDto> MapCreatedFloors(IReadOnlyList<Unit> units)
    {
        // Response floors are derived from created unit floor numbers; Haven does not persist a Floors table.
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
        // Create response stays compact; floor rows are grouped from the persisted submitted rooms.
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
        // Group tenants by room before mapping room cards, so each tenant row is projected once.
        var tenantLookup = roomTenants
            .GroupBy(x => x.UnitPublicId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(tenant => tenant.Adapt<PropertyListRoomTenantResponseDto>()).ToList());
        // Group mapped room cards by property and floor for direct attachment to each floor response.
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
        // Group mapped room rows once so each floor can attach its persisted room response without repeated scans.
        var roomLookup = rooms
            .GroupBy(x => (x.PropertyPublicId, x.FloorNumber))
            .ToDictionary(
                x => x.Key,
                x => x.Select(room => room.Adapt<PropertyDetailRoomResponseDto>()).ToList());

        // Assemble the detail-floor hierarchy by property after room grouping is complete.
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
    /// Maps property-level package rows into package templates for the detail response.
    /// </summary>
    /// <param name="rows">The package template source rows.</param>
    /// <returns>The distinct package templates.</returns>
    private static IReadOnlyList<PropertyPackageTemplateResponseDto> MapPackageTemplates(
        IReadOnlyList<PropertyPackageTemplateRowModel> rows)
    {
        // Property detail returns the common package templates; room overrides belong to room detail.
        return rows
            .GroupBy(x => new { x.PackagePublicId, x.PackageName, x.PriceAdjustment })
            .OrderBy(x => x.Key.PackageName)
            .Select(x =>
            {
                var package = x.First().Adapt<PropertyPackageTemplateResponseDto>();

                // Reconstruct each package's ordered item list from the repeated package/item read rows.
                package.Items = x
                    .Where(item => !string.IsNullOrWhiteSpace(item.ItemName))
                    .OrderBy(item => item.DisplayOrder ?? int.MaxValue)
                    .ThenBy(item => item.ItemName)
                    .Select(item => item.Adapt<PropertyPackageTemplateItemResponseDto>())
                    .DistinctBy(item => item.Name)
                    .ToList();
                return package;
            })
            .ToList();
    }

    /// <summary>
    /// Maps the whole-building rental section only when the detail UI should render it.
    /// </summary>
    /// <param name="row">The representative building-unit row with optional contract data.</param>
    /// <returns>The whole-building section, or <c>null</c> when normal room rentals exist.</returns>
    private static PropertyWholeBuildingRentalResponseDto MapWholeBuildingRental(PropertyWholeBuildingRentalRowModel row)
    {
        // Normal multi-room properties do not render the whole-building rental card.
        if (row is null)
        {
            return null;
        }

        // Whole-building rental is still room-backed, but detail UI needs only the compact contract card.
        if (row.ContractPublicId.HasValue)
        {
            return new PropertyWholeBuildingRentalResponseDto
            {
                Contract = row.Adapt<PropertyWholeBuildingRentalContractResponseDto>()
            };
        }

        return new PropertyWholeBuildingRentalResponseDto();
    }
}
