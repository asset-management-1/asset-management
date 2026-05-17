namespace Authentication.Application.Helpers;

/// <summary>
/// Maps persisted master-data codes into public API enum contract values.
/// </summary>
public static class ApiEnumContractMapper
{
    /// <summary>
    /// Converts a gender master-data value into the public gender enum.
    /// </summary>
    /// <param name="value">The gender code or name from persistence.</param>
    /// <returns>The matching gender enum; otherwise <c>null</c>.</returns>
    public static GenderEnum? ToGender(string value)
    {
        // Profile gender is optional, so missing master data remains null in the public response.
        return value switch
        {
            { } gender when Matches(gender, nameof(GenderEnum.Male)) => GenderEnum.Male,
            { } gender when Matches(gender, nameof(GenderEnum.Female)) => GenderEnum.Female,
            _ => null
        };
    }

    /// <summary>
    /// Converts a party context code or name into the public party-type enum.
    /// </summary>
    /// <param name="value">The party context value from persistence or request mapping.</param>
    /// <returns>The matching party-type enum; otherwise <c>null</c>.</returns>
    public static PartyTypeEnum? ToPartyType(string value)
    {
        // Context values may arrive as UI context strings or persisted master-data codes.
        return value switch
        {
            { } context when Matches(context, nameof(PartyTypeEnum.Tenant)) => PartyTypeEnum.Tenant,
            { } context when Matches(context, nameof(PartyTypeEnum.Landlord)) => PartyTypeEnum.Landlord,
            _ => null
        };
    }

    /// <summary>
    /// Converts context values into public party-type enum values.
    /// </summary>
    /// <param name="values">The context values to convert.</param>
    /// <returns>The matching party-type enum values.</returns>
    public static List<PartyTypeEnum> ToPartyTypes(IEnumerable<string> values)
    {
        // Preserve the current behavior of dropping unknown linked contexts from response collections.
        return values?
            .Select(ToPartyType)
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .ToList() ?? [];
    }

    /// <summary>
    /// Converts an identifier type master-data value into the public identifier enum.
    /// </summary>
    /// <param name="value">The identifier type code or name from persistence.</param>
    /// <returns>The matching identifier type enum; otherwise <c>null</c>.</returns>
    public static IdentifierTypeEnum? ToIdentifierType(string value)
    {
        // KYC may be absent for a profile, so missing identifier type remains null.
        return value switch
        {
            { } identifierType when Matches(identifierType, nameof(IdentifierTypeEnum.Cccd)) => IdentifierTypeEnum.Cccd,
            { } identifierType when Matches(identifierType, nameof(IdentifierTypeEnum.Passport)) => IdentifierTypeEnum.Passport,
            _ => null
        };
    }

    /// <summary>
    /// Converts a KYC status master-data value into the public KYC status enum.
    /// </summary>
    /// <param name="value">The KYC status code or name from persistence.</param>
    /// <returns>The matching KYC status enum; otherwise <c>null</c>.</returns>
    public static KycStatusEnum? ToKycStatus(string value)
    {
        // KYC status is nullable on user-info until the user submits identity data.
        return value switch
        {
            { } status when Matches(status, nameof(KycStatusEnum.Pending)) => KycStatusEnum.Pending,
            { } status when Matches(status, nameof(KycStatusEnum.Approved)) => KycStatusEnum.Approved,
            { } status when Matches(status, nameof(KycStatusEnum.Rejected)) => KycStatusEnum.Rejected,
            _ => null
        };
    }

    /// <summary>
    /// Converts a vehicle type master-data value into the public vehicle type enum.
    /// </summary>
    /// <param name="value">The vehicle type code or name from persistence.</param>
    /// <returns>The matching vehicle type enum; otherwise <c>null</c>.</returns>
    public static VehicleTypeEnum? ToVehicleType(string value)
    {
        // Vehicle type responses use the same enum set as vehicle registration requests.
        return value switch
        {
            { } vehicleType when Matches(vehicleType, nameof(VehicleTypeEnum.Car)) => VehicleTypeEnum.Car,
            { } vehicleType when Matches(vehicleType, nameof(VehicleTypeEnum.Motorbike)) => VehicleTypeEnum.Motorbike,
            { } vehicleType when Matches(vehicleType, nameof(VehicleTypeEnum.Bicycle)) => VehicleTypeEnum.Bicycle,
            _ => null
        };
    }

    /// <summary>
    /// Converts a vehicle type master-data value into the required public vehicle type enum.
    /// </summary>
    /// <param name="value">The vehicle type code or name from persistence.</param>
    /// <returns>The matching vehicle type enum.</returns>
    public static VehicleTypeEnum ToRequiredVehicleType(string value)
    {
        // Active vehicle rows must have a supported type; fail fast if master data drifts.
        return ToVehicleType(value)
               ?? throw new InvalidOperationException(string.Format(UNSUPPORTED_VEHICLE_TYPE_VALUE_MESSAGE, value));
    }

    /// <summary>
    /// Converts a vehicle status value into the public vehicle status enum.
    /// </summary>
    /// <param name="value">The vehicle status code or name.</param>
    /// <returns>The matching vehicle status enum.</returns>
    public static UserVehicleStatusEnum ToRequiredVehicleStatus(string value)
    {
        // Current vehicle APIs expose only active rows today.
        if (Matches(value, nameof(UserVehicleStatusEnum.Active)))
        {
            return UserVehicleStatusEnum.Active;
        }

        throw new InvalidOperationException(string.Format(UNSUPPORTED_VEHICLE_STATUS_VALUE_MESSAGE, value));
    }

    /// <summary>
    /// Compares internal code values without leaking DB casing into API contracts.
    /// </summary>
    /// <param name="value">The source value to compare.</param>
    /// <param name="expected">The expected internal value.</param>
    /// <returns><c>true</c> when values match ignoring case and surrounding whitespace.</returns>
    private static bool Matches(string value, string expected)
    {
        // Master-data codes and UI context strings differ by casing, so comparisons stay case-insensitive.
        return string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
    }
}
