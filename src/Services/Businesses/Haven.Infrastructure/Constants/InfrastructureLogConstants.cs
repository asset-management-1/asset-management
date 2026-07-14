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
        /// Logged after a property and its submitted unit graph are persisted.
        /// </summary>
        public const string PROPERTY_CREATED = "Created property {PropertyPublicId} with {TotalUnits} submitted rooms for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a property edit graph is persisted.
        /// </summary>
        public const string PROPERTY_UPDATED = "Updated property {PropertyPublicId} for party {PartyPublicId}.";

        /// <summary>
        /// Logged before a submitted property update is rejected by preflight guards.
        /// </summary>
        public const string PROPERTY_UPDATE_PREFLIGHT_REJECTED = "Rejected property update {PropertyPublicId} for party {PartyPublicId}. Reason: {Reason}.";

        /// <summary>
        /// Logged after a property delete is scheduled.
        /// </summary>
        public const string PROPERTY_DELETED = "Scheduled property {PropertyPublicId} for delete at {DeleteScheduledAt} by party {PartyPublicId}.";

        /// <summary>
        /// Logged after a pending property delete request is restored.
        /// </summary>
        public const string PROPERTY_DELETE_RESTORED = "Restored pending delete for property {PropertyPublicId} by party {PartyPublicId}.";
    }

    /// <summary>
    /// Contains log messages related to room list, detail, update, and delete workflows.
    /// </summary>
    public static class RoomLogs
    {
        /// <summary>
        /// Logged after a room list read completes.
        /// </summary>
        public const string ROOMS_LOADED = "Loaded {Count} room rows for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a room detail read completes.
        /// </summary>
        public const string ROOM_DETAIL_LOADED = "Loaded room detail {RoomPublicId} for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a room edit graph is persisted.
        /// </summary>
        public const string ROOM_UPDATED = "Updated room {RoomPublicId} for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a room is soft-deleted.
        /// </summary>
        public const string ROOM_DELETED = "Deleted room {RoomPublicId} for party {PartyPublicId}.";
    }

    /// <summary>
    /// Contains log messages related to tenant list, detail, join, and move-out workflows.
    /// </summary>
    public static class TenantLogs
    {
        /// <summary>
        /// Logged after a tenant list read completes.
        /// </summary>
        public const string TENANTS_LOADED = "Loaded {Count} tenant rows for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a tenant detail read completes.
        /// </summary>
        public const string TENANT_DETAIL_LOADED = "Loaded tenant occupancy {OccupancyPublicId} for party {PartyPublicId}.";

        /// <summary>
        /// Logged after a tenant occupancy is created.
        /// </summary>
        public const string TENANT_CREATED = "Created tenant occupancy {OccupancyPublicId} for room {RoomPublicId} by party {PartyPublicId}.";

        /// <summary>
        /// Logged after a tenant occupancy is moved out.
        /// </summary>
        public const string TENANT_MOVED_OUT = "Moved out tenant occupancy {OccupancyPublicId} by party {PartyPublicId}.";

        /// <summary>
        /// Logged after a room tenant-join QR token is created.
        /// </summary>
        public const string TENANT_JOIN_QR_CREATED = "Created tenant join QR for room {RoomPublicId} by party {PartyPublicId}.";

        /// <summary>
        /// Logged when a consumed tenant-join token cannot be restored after occupancy creation fails.
        /// </summary>
        public const string TENANT_JOIN_TOKEN_RESTORE_FAILED = "Tenant join failed after token consumption, and the token could not be restored for room {RoomPublicId}.";
    }

    /// <summary>
    /// Contains vehicle registration log templates.
    /// </summary>
    public static class VehicleLogs
    {
        public const string VEHICLES_LOADED = "Loaded {Count} vehicle rows for party {PartyPublicId}.";
        public const string VEHICLE_CREATED = "Created vehicle {VehiclePublicId} for party {PartyPublicId}.";
        public const string VEHICLE_UPDATED = "Updated vehicle {VehiclePublicId} for party {PartyPublicId}.";
        public const string VEHICLE_DELETED = "Deleted vehicle {VehiclePublicId} for party {PartyPublicId}.";
        public const string VEHICLE_IMAGE_UPLOAD_FAILED = "Vehicle image upload failed for vehicle {VehiclePublicId}.";
        public const string VEHICLE_IMAGE_CLEANUP_FAILED = "Vehicle image cleanup failed for vehicle {VehiclePublicId}.";
    }

    /// <summary>
    /// Contains log messages for standalone room package reads and mutations.
    /// </summary>
    public static class RoomPackageLogs
    {
        /// <summary>
        /// Logged after one room's effective package list is loaded.
        /// </summary>
        public const string ROOM_PACKAGES_LOADED = "Loaded {Count} effective packages for room {RoomPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Logged after a room-owned package is created.
        /// </summary>
        public const string ROOM_PACKAGE_CREATED = "Created room package {PackagePublicId} for room {RoomPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Logged after a room-owned package is updated.
        /// </summary>
        public const string ROOM_PACKAGE_UPDATED = "Updated room package {PackagePublicId} for room {RoomPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Logged after a room-owned package is soft-deleted.
        /// </summary>
        public const string ROOM_PACKAGE_DELETED = "Deleted room package {PackagePublicId} for room {RoomPublicId} and party {PartyPublicId}.";
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
        public const string LANDLORD_CONTEXT_REJECTED = "Current party {PartyPublicId} rejected for landlord flow. PartyType={PartyTypeCode}. Status={StatusCode}.";
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
        public const string MASTER_DATA_VALUE_MISSING = "Master-data value missing for type {Type} and code {Code}.";
    }

    /// <summary>
    /// Contains log messages related to province, district, and ward location validation.
    /// </summary>
    public static class LocationLogs
    {
        /// <summary>
        /// Logged after location codes are resolved into a hierarchy context.
        /// </summary>
        public const string LOCATION_CONTEXT_LOADED = "Resolved location context for ProvinceCode={ProvinceCode}, DistrictCode={DistrictCode}, WardCode={WardCode}.";

        /// <summary>
        /// Logged before an invalid supplied location code is returned as a bad request.
        /// </summary>
        public const string LOCATION_CODE_MISSING = "Location code missing for kind {LocationKind} and code {Code}.";
    }

    /// <summary>Contains structured utility workflow log templates.</summary>
    public static class MeterLogs
    {
        public const string PERIOD_LOADED = "Loaded utility period {BillingMonth} for property {PropertyId} and room {RoomId}.";
        public const string EVIDENCE_CLEANUP_FAILED = "Utility evidence cleanup failed for property {PropertyId}.";
    }
}
