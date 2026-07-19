namespace Haven.Application.Constants;

/// <summary>
/// Stores Haven application-layer log message templates.
/// </summary>
public static class ApplicationLogConstants
{
    /// <summary>
    /// Contains property command and query log templates.
    /// </summary>
    public static class PropertyLogs
    {
        /// <summary>
        /// Log template for property list query completion at the application boundary.
        /// </summary>
        public const string PROPERTY_LIST_QUERY_COMPLETED = "Property list query completed for party {PartyPublicId}.";

        /// <summary>
        /// Log template for property detail query completion at the application boundary.
        /// </summary>
        public const string PROPERTY_DETAIL_QUERY_COMPLETED = "Property detail query completed for property {PropertyPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for property creation command completion at the application boundary.
        /// </summary>
        public const string PROPERTY_CREATE_COMMAND_COMPLETED = "Property create command completed for property {PropertyPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for property update command completion at the application boundary.
        /// </summary>
        public const string PROPERTY_UPDATE_COMMAND_COMPLETED = "Property update command completed for property {PropertyPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for property delete command completion at the application boundary.
        /// </summary>
        public const string PROPERTY_DELETE_COMMAND_COMPLETED = "Property delete command completed for property {PropertyPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for property restore-delete command completion at the application boundary.
        /// </summary>
        public const string PROPERTY_RESTORE_DELETE_COMMAND_COMPLETED = "Property restore-delete command completed for property {PropertyPublicId} and party {PartyPublicId}.";
    }

    /// <summary>
    /// Contains room command and query log templates.
    /// </summary>
    public static class RoomLogs
    {
        /// <summary>
        /// Log template for room list query completion at the application boundary.
        /// </summary>
        public const string ROOM_LIST_QUERY_COMPLETED = "Room list query completed for party {PartyPublicId}.";

        /// <summary>
        /// Log template for room detail query completion at the application boundary.
        /// </summary>
        public const string ROOM_DETAIL_QUERY_COMPLETED = "Room detail query completed for room {RoomPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for room update command completion at the application boundary.
        /// </summary>
        public const string ROOM_UPDATE_COMMAND_COMPLETED = "Room update command completed for room {RoomPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for room delete command completion at the application boundary.
        /// </summary>
        public const string ROOM_DELETE_COMMAND_COMPLETED = "Room delete command completed for room {RoomPublicId} and party {PartyPublicId}.";
    }

    /// <summary>
    /// Contains standalone room package command and query log templates.
    /// </summary>
    public static class RoomPackageLogs
    {
        public const string ROOM_PACKAGE_LIST_QUERY_COMPLETED = "Room package list query completed for room {RoomPublicId} and party {PartyPublicId}.";
        public const string ROOM_PACKAGE_DETAIL_QUERY_COMPLETED = "Room package detail query completed for package {PackagePublicId}, room {RoomPublicId}, and party {PartyPublicId}.";
        public const string ROOM_PACKAGE_CREATE_COMMAND_COMPLETED = "Room package create command completed for package {PackagePublicId}, room {RoomPublicId}, and party {PartyPublicId}.";
        public const string ROOM_PACKAGE_UPDATE_COMMAND_COMPLETED = "Room package update command completed for package {PackagePublicId}, room {RoomPublicId}, and party {PartyPublicId}.";
        public const string ROOM_PACKAGE_DELETE_COMMAND_COMPLETED = "Room package delete command completed for package {PackagePublicId}, room {RoomPublicId}, and party {PartyPublicId}.";
    }

    /// <summary>
    /// Contains tenant command, query, and QR join log templates.
    /// </summary>
    public static class TenantLogs
    {
        /// <summary>
        /// Log template for tenant list query completion at the application boundary.
        /// </summary>
        public const string TENANT_LIST_QUERY_COMPLETED = "Tenant list query completed for party {PartyPublicId}.";

        /// <summary>
        /// Log template for tenant detail query completion at the application boundary.
        /// </summary>
        public const string TENANT_DETAIL_QUERY_COMPLETED = "Tenant detail query completed for occupancy {OccupancyPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for tenant create command completion at the application boundary.
        /// </summary>
        public const string TENANT_CREATE_COMMAND_COMPLETED = "Tenant create command completed for occupancy {OccupancyPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for tenant delete command completion at the application boundary.
        /// </summary>
        public const string TENANT_DELETE_COMMAND_COMPLETED = "Tenant delete command completed for occupancy {OccupancyPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for tenant join QR query completion at the application boundary.
        /// </summary>
        public const string TENANT_JOIN_QR_QUERY_COMPLETED = "Tenant join QR generated for room {RoomPublicId} and party {PartyPublicId}.";

        /// <summary>
        /// Log template for tenant join preview query completion at the application boundary.
        /// </summary>
        public const string TENANT_JOIN_PREVIEW_QUERY_COMPLETED = "Tenant join preview loaded.";

        /// <summary>
        /// Log template for tenant join confirm command completion at the application boundary.
        /// </summary>
        public const string TENANT_JOIN_CONFIRM_COMMAND_COMPLETED = "Tenant join confirm completed for occupancy {OccupancyPublicId}.";
    }

    /// <summary>
    /// Contains vehicle command and query log templates.
    /// </summary>
    public static class VehicleLogs
    {
        public const string VEHICLE_LIST_QUERY_COMPLETED = "Vehicle list query completed for party {PartyPublicId}.";
        public const string VEHICLE_DETAIL_QUERY_COMPLETED = "Vehicle detail query completed for vehicle {VehiclePublicId} and party {PartyPublicId}.";
        public const string VEHICLE_CREATE_COMMAND_COMPLETED = "Vehicle create command completed for vehicle {VehiclePublicId} and party {PartyPublicId}.";
        public const string VEHICLE_UPDATE_COMMAND_COMPLETED = "Vehicle update command completed for vehicle {VehiclePublicId} and party {PartyPublicId}.";
        public const string VEHICLE_DELETE_COMMAND_COMPLETED = "Vehicle delete command completed for vehicle {VehiclePublicId} and party {PartyPublicId}.";
    }

    /// <summary>Contains meter record command and query log templates.</summary>
    public static class MeterLogs
    {
        /// <summary>
        /// Logs completion of a room meter mutation.
        /// </summary>
        public const string MUTATION_COMPLETED = "Meter mutation completed for room {RoomId} on {BillingDate}.";

        /// <summary>
        /// Logs completion of a bounded room meter history read.
        /// </summary>
        public const string HISTORY_LOADED = "Loaded meter history for room {RoomId} in {Year}.";

        /// <summary>
        /// Logs completion of one room meter period read.
        /// </summary>
        public const string PERIOD_LOADED = "Loaded meter period for room {RoomId} in {Month}/{Year}.";
    }
}
