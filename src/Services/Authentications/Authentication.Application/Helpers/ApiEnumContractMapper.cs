namespace Authentication.Application.Helpers;

/// <summary>
/// Maps persisted master-data codes into public API enum contract values.
/// </summary>
public static class ApiEnumContractMapper
{
    /// <summary>
    /// Converts a gender master-data value into the public gender enum.
    /// </summary>
    /// <param name="value">The canonical gender code from persistence.</param>
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
    /// Converts a canonical party context code into the public party-type enum.
    /// </summary>
    /// <param name="value">The canonical party context code.</param>
    /// <returns>The matching party-type enum; otherwise <c>null</c>.</returns>
    public static PartyTypeEnum? ToPartyType(string value)
    {
        // Request and persistence boundaries supply canonical codes; display names are never business inputs.
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
    /// <param name="value">The canonical identifier type code from persistence.</param>
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
    /// <param name="value">The canonical KYC status code from persistence.</param>
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
    /// Compares internal code values without leaking DB casing into API contracts.
    /// </summary>
    /// <param name="value">The source value to compare.</param>
    /// <param name="expected">The expected internal value.</param>
    /// <returns><c>true</c> when canonical code values match ignoring case.</returns>
    private static bool Matches(string value, string expected)
    {
        // Enum member casing differs from persisted uppercase codes; no display-name or whitespace fallback is allowed.
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
