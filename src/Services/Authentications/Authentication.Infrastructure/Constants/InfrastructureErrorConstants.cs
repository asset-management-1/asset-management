namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Stores authentication infrastructure error messages grouped by technical owner.
/// </summary>
public static class InfrastructureErrorConstants
{
    /// <summary>
    /// Contains party-context mapping error messages.
    /// </summary>
    public static class PartyContextErrors
    {
        /// <summary>
        /// Error message used when a party context value cannot be mapped to the public enum contract.
        /// </summary>
        public const string UNSUPPORTED_PARTY_CONTEXT_VALUE_MESSAGE = "Unsupported party context value '{0}'.";
    }
}
