namespace Haven.Infrastructure.Constants;

/// <summary>
/// Contains structured log-message constants used by the Haven business infrastructure layer.
/// </summary>
public static class InfrastructureLogConstants
{
    /// <summary>
    /// Contains log messages related to property list, detail, and creation workflows.
    /// </summary>
    public static class PropertyLogs
    {
        /// <summary>
        /// Logged after a property list read completes.
        /// </summary>
        public const string PROPERTIES_LOADED = "Loaded {Count} properties for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a property detail read completes.
        /// </summary>
        public const string PROPERTY_DETAIL_LOADED = "Loaded property detail {PropertyPublicId} for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a property and its generated unit graph are persisted.
        /// </summary>
        public const string PROPERTY_CREATED =
            "Created property {PropertyPublicId} with {TotalUnits} generated units for party {PartyPublicId}.";
    }

    /// <summary>
    /// Contains log messages related to current-party context resolution.
    /// </summary>
    public static class PartyLogs
    {
        /// <summary>
        /// Logged when a current-party lookup has no authenticated user.
        /// </summary>
        public const string CURRENT_PARTY_USER_MISSING = "Current party lookup rejected because authenticated user is missing.";

        /// <summary>
        /// Logged when an authenticated user has no selected current party.
        /// </summary>
        public const string CURRENT_PARTY_MISSING = "Current party context missing for user {UserPublicId}.";

        /// <summary>
        /// Logged after a current party context is loaded.
        /// </summary>
        public const string CURRENT_PARTY_LOADED = "Current party {PartyPublicId} loaded for user {UserPublicId}.";

        /// <summary>
        /// Logged when a property-management flow rejects a non-active-landlord context.
        /// </summary>
        public const string LANDLORD_CONTEXT_REJECTED =
            "Current party {PartyPublicId} rejected for landlord flow. PartyType={PartyTypeCode}. Status={StatusCode}.";
    }

    /// <summary>
    /// Contains log messages related to master-data validation.
    /// </summary>
    public static class MasterDataLogs
    {
        /// <summary>
        /// Logged after all requested master-data keys are validated.
        /// </summary>
        public const string MASTER_DATA_VALUES_VALIDATED = "Validated {Count} master-data values.";

        /// <summary>
        /// Logged before an unresolved master-data key is returned as a bad request.
        /// </summary>
        public const string MASTER_DATA_VALUE_MISSING =
            "Master-data value missing for type {Type} and code {Code}.";
    }

    /// <summary>
    /// Contains log messages related to province, district, and ward location validation.
    /// </summary>
    public static class LocationLogs
    {
        /// <summary>
        /// Logged after location codes are resolved into a hierarchy context.
        /// </summary>
        public const string LOCATION_CONTEXT_LOADED =
            "Resolved location context. ProvinceFound={ProvinceFound}. DistrictFound={DistrictFound}. WardFound={WardFound}.";

        /// <summary>
        /// Logged before an invalid supplied location code is returned as a bad request.
        /// </summary>
        public const string LOCATION_CODE_MISSING = "Location code missing for kind {LocationKind} and code {Code}.";
    }
}
