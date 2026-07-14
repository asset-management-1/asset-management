namespace Haven.Infrastructure.Mappings.Rooms;

/// <summary>
/// Applies shared room field mutations to tracked unit entities.
/// </summary>
public static class RoomFieldMutationMapper
{
    /// <summary>
    /// Checks whether submitted room fields would change protected rental structure or money fields.
    /// </summary>
    /// <param name="room">The tracked room entity.</param>
    /// <param name="fields">The submitted room fields.</param>
    /// <param name="lookups">The resolved lookup identifiers needed by submitted codes.</param>
    /// <returns><c>true</c> when a protected field changes.</returns>
    public static bool HasProtectedFieldChanges(
        Unit room,
        RoomFieldUpdateModel fields,
        RoomFieldLookupModel lookups)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(lookups);

        // Only values present in the request can produce protected changes; omitted fields keep DB state.
        var hasFloorChange = fields.FloorNumber.HasValue
                             && room.FloorNumber != fields.FloorNumber.Value;
        var hasTypeChange = fields.TypeCode is not null
                            && lookups.UnitTypeId.HasValue
                            && room.UnitTypeId != lookups.UnitTypeId.Value;
        var hasRentalModeChange = fields.RentalModeCode is not null
                                  && lookups.RentalModeId.HasValue
                                  && room.RentalModeId != lookups.RentalModeId.Value;

        // Money and capacity changes are blocked for rooms that already have contract/occupancy data.
        var hasCapacityChange = fields.TotalBeds.HasValue
                                && room.BedCount != fields.TotalBeds.Value;
        var hasRentChange = fields.BaseRentAmount.HasValue
                            && room.BaseRentAmount != fields.BaseRentAmount.Value;
        var hasDepositChange = fields.DefaultDepositAmount.HasValue
                               && room.DefaultDepositAmount != fields.DefaultDepositAmount.Value;

        return hasFloorChange
               || hasTypeChange
               || hasRentalModeChange
               || hasCapacityChange
               || hasRentChange
               || hasDepositChange;
    }

    /// <summary>
    /// Applies submitted room fields to a tracked unit entity.
    /// </summary>
    /// <param name="room">The tracked room entity.</param>
    /// <param name="fields">The submitted room fields.</param>
    /// <param name="lookups">The resolved lookup identifiers needed by submitted codes.</param>
    public static void Apply(
        Unit room,
        RoomFieldUpdateModel fields,
        RoomFieldLookupModel lookups)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(lookups);

        // Mapster owns the flat partial field copy; master-data backed fields stay explicit below.
        fields.Adapt(room);

        if (fields.TypeCode is not null && lookups.UnitTypeId.HasValue)
        {
            room.UnitTypeId = lookups.UnitTypeId.Value;
        }

        if (fields.RentalModeCode is not null && lookups.RentalModeId.HasValue)
        {
            room.RentalModeId = lookups.RentalModeId.Value;
            if (!string.Equals(
                fields.RentalModeCode,
                MASTER_CODE_RENTAL_MODE_SHARED_BED,
                StringComparison.OrdinalIgnoreCase))
            {
                // Bed capacity only applies to KTX/shared-bed rooms; other rental modes clear the capacity.
                room.BedCount = null;
            }
        }
    }
}
