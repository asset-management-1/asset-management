namespace Authentication.Domain.Enums;

/// <summary>
/// Represents supported party contexts used by authentication request flows.
/// </summary>
public enum PartyTypeEnum
{
    /// <summary>
    /// Tenant party context.
    /// </summary>
    Tenant = 1,

    /// <summary>
    /// Landlord party context.
    /// </summary>
    Landlord = 2
}
