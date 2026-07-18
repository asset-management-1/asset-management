namespace Be.Haven.Shared.Constants.Validation;

/// <summary>
/// Contains reusable regular-expression patterns for transport-independent validation.
/// </summary>
public static class ValidationPattern
{
    /// <summary>
    /// Matches canonical E.164 phone numbers with a leading plus sign and 8 to 15 digits.
    /// </summary>
    public const string E164_PHONE_NUMBER = @"^\+[1-9]\d{7,14}$";
}
