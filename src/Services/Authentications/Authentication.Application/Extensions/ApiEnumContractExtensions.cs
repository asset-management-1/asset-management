namespace Authentication.Application.Extensions;

/// <summary>
/// Provides API enum contract conversions used before values cross into persistence-shaped DTOs.
/// </summary>
public static class ApiEnumContractExtensions
{
    /// <summary>
    /// Converts a party-type enum value into the lowercase API context value used internally.
    /// </summary>
    /// <param name="value">The party type enum value.</param>
    /// <returns>The lowercase API context value, or <c>null</c> when unsupported.</returns>
    public static string ToContextValue(this PartyTypeEnum value)
    {
        // Context request DTOs historically use lowercase strings between application and infrastructure layers.
        return Enum.IsDefined(value) ? value.ToString().ToLowerInvariant() : null;
    }
}
