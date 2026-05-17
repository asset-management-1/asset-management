namespace Authentication.Domain.Enums;

/// <summary>
/// Represents supported personal identity document types for user KYC submissions.
/// </summary>
public enum IdentifierTypeEnum
{
    /// <summary>
    /// Citizen identity card / CCCD.
    /// </summary>
    Cccd = 1,

    /// <summary>
    /// Passport identity document.
    /// </summary>
    Passport = 2
}
