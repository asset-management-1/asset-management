namespace Haven.Application.Extensions;

/// <summary>
/// Provides use-case shaping helpers for property creation requests.
/// </summary>
public static class PropertyCreationRequestExtensions
{
    /// <summary>
    /// Gets the master-data keys needed to create the property graph.
    /// </summary>
    /// <param name="request">The property creation request.</param>
    /// <returns>The distinct master-data keys needed by the create flow.</returns>
    public static IReadOnlyCollection<MasterDataKeyModel> GetMasterDataKeys(this PropertyCreationRequestModel request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var structure = request.StructureSetup;
        var keys = new HashSet<MasterDataKeyModel>();

        keys.AddKey(MASTER_TYPE_PROPERTY_TYPE, request.PropertyTypeCode);
        keys.AddKey(MASTER_TYPE_PROPERTY_STATUS, MASTER_CODE_PROPERTY_STATUS_DRAFT);
        AddUnitSetupKeys(keys, structure);
        keys.AddKey(MASTER_TYPE_UNIT_STATUS, MASTER_CODE_UNIT_STATUS_AVAILABLE);
        keys.AddKey(MASTER_TYPE_UNIT_PACKAGE_TYPE, MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE);
        keys.AddKey(MASTER_TYPE_UNIT_PACKAGE_STATUS, MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE);
        keys.AddKey(MASTER_TYPE_PROPERTY_RELATIONSHIP_TYPE, PropertyRelationshipTypeEnum.Landlord.ToMasterDataCode());
        keys.AddKey(MASTER_TYPE_COMMON_STATUS, MASTER_CODE_ACTIVE);

        if ((request.Packages ?? []).Count > 0)
        {
            keys.AddKey(MASTER_TYPE_UNIT_PACKAGE_TYPE, MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM);
        }

        foreach (var chargePolicy in request.ChargePolicies ?? [])
        {
            keys.AddKey(MASTER_TYPE_INVOICE_LINE_TYPE, chargePolicy.Code);
            if (string.Equals(
                    chargePolicy.Code,
                    MASTER_CODE_INVOICE_LINE_TYPE_PARKING,
                    StringComparison.OrdinalIgnoreCase))
            {
                keys.AddKey(MASTER_TYPE_VEHICLE_TYPE, chargePolicy.VehicleTypeCode);
            }
        }

        return keys;
    }

    /// <summary>
    /// Adds the unit-level master-data keys for quick or explicit structure setup.
    /// </summary>
    /// <param name="keys">The key set being built.</param>
    /// <param name="structure">The property structure setup.</param>
    private static void AddUnitSetupKeys(
        ISet<MasterDataKeyModel> keys,
        CreatePropertyStructureRequestDto structure)
    {
        if ((structure.Floors ?? []).Count == 0)
        {
            // Quick setup uses shared defaults for every generated room.
            keys.AddKey(MASTER_TYPE_UNIT_TYPE, structure.DefaultUnitTypeCode);
            keys.AddKey(MASTER_TYPE_UNIT_RENTAL_MODE, structure.DefaultRentalModeCode);
            return;
        }

        // Explicit setup resolves the exact code values supplied per room by the UI.
        foreach (var room in structure.Floors.SelectMany(floor => floor.Rooms ?? []))
        {
            keys.AddKey(MASTER_TYPE_UNIT_TYPE, room.TypeCode);
            keys.AddKey(MASTER_TYPE_UNIT_RENTAL_MODE, room.RentalModeCode);
        }
    }
}
