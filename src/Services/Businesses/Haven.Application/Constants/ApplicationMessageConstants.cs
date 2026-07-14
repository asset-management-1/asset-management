namespace Haven.Application.Constants;

/// <summary>
/// Stores Haven application user-facing success and status messages grouped by owning workflow.
/// </summary>
public static class ApplicationMessageConstants
{
    /// <summary>
    /// Contains user-facing property operation messages.
    /// </summary>
    public static class PropertyMessages
    {
        /// <summary>
        /// Success message returned after a property delete is scheduled.
        /// </summary>
        public const string PROPERTY_DELETE_SUCCESS_MESSAGE = "Property delete has been scheduled.";

        /// <summary>
        /// Success message returned after pending property delete is restored.
        /// </summary>
        public const string PROPERTY_RESTORE_DELETE_SUCCESS_MESSAGE = "Property delete request restored successfully.";
    }

    /// <summary>
    /// Contains user-facing room operation messages.
    /// </summary>
    public static class RoomMessages
    {
        /// <summary>
        /// Success message returned after a room is deleted.
        /// </summary>
        public const string ROOM_DELETE_SUCCESS_MESSAGE = "Room deleted successfully.";
    }

    /// <summary>
    /// Contains room package operation messages.
    /// </summary>
    public static class RoomPackageMessages
    {
        public const string ROOM_PACKAGE_DELETE_SUCCESS_MESSAGE = "Room package deleted successfully.";
    }

    /// <summary>
    /// Contains user-facing tenant operation messages.
    /// </summary>
    public static class TenantMessages
    {
        /// <summary>
        /// Success message returned after a tenant occupancy is moved out.
        /// </summary>
        public const string TENANT_DELETE_SUCCESS_MESSAGE = "Tenant moved out successfully.";
    }

    /// <summary>
    /// Contains user-facing vehicle operation messages.
    /// </summary>
    public static class VehicleMessages
    {
        public const string VEHICLE_DELETE_SUCCESS_MESSAGE = "Vehicle deleted successfully.";
    }

}
