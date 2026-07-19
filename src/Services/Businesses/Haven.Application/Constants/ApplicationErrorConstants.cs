namespace Haven.Application.Constants;

/// <summary>
/// Stores Haven application error messages and error codes grouped by owning workflow.
/// </summary>
public static class ApplicationErrorConstants
{
    /// <summary>
    /// Contains errors returned by standalone room package workflows.
    /// </summary>
    public static class RoomPackageErrors
    {
        /// <summary>
        /// Error returned when a room package is outside the current room scope.
        /// </summary>
        public const string ERROR_ROOM_PACKAGE_NOT_FOUND = "Room package was not found in the current room scope.";

        /// <summary>
        /// Error returned when contract dependencies block package mutation.
        /// </summary>
        public const string ERROR_ROOM_PACKAGE_MUTATION_BLOCKED = "Room package cannot be changed because it is referenced by a contract.";

        /// <summary>
        /// Error returned when the system fallback package is mutated.
        /// </summary>
        public const string ERROR_ROOM_PACKAGE_DEFAULT_IMMUTABLE = "The default no-furniture package cannot be changed or deleted.";

        /// <summary>
        /// Error returned when package item names are duplicated.
        /// </summary>
        public const string ERROR_ROOM_PACKAGE_ITEMS_DUPLICATED = "Room package item names must be unique.";

        /// <summary>
        /// Error returned when the room package limit is exceeded.
        /// </summary>
        public const string ERROR_ROOM_PACKAGE_LIMIT_EXCEEDED = "The room package limit has been exceeded.";

        /// <summary>
        /// Error returned when an unused package code cannot be generated.
        /// </summary>
        public const string ERROR_ROOM_PACKAGE_CODE_GENERATION_FAILED = "A room package code could not be generated.";
    }

    /// <summary>
    /// Contains cross-cutting current-party and authenticated-context errors.
    /// </summary>
    public static class ContextErrors
    {
        /// <summary>
        /// Error message used when the authenticated user cannot be resolved.
        /// </summary>
        public const string ERROR_AUTHENTICATED_USER_REQUIRED = "Authenticated user is required.";

        /// <summary>
        /// Error message used when the current party context is missing.
        /// </summary>
        public const string ERROR_CURRENT_PARTY_REQUIRED = "Current party context is required.";

        /// <summary>
        /// Error message used when landlord-only management is attempted outside landlord context.
        /// </summary>
        public const string ERROR_LANDLORD_CONTEXT_REQUIRED = "Current party must be an active landlord context to manage properties.";
    }

    /// <summary>
    /// Contains lookup errors for master data and locations.
    /// </summary>
    public static class LookupErrors
    {
        /// <summary>
        /// Error message template used when a master-data value cannot be resolved.
        /// </summary>
        public const string ERROR_MASTER_DATA_NOT_FOUND = "Master-data value '{0}:{1}' was not found or inactive.";

        /// <summary>
        /// Error message template used when a location code cannot be resolved.
        /// </summary>
        public const string ERROR_LOCATION_NOT_FOUND = "Location code '{0}:{1}' was not found.";
    }

    /// <summary>
    /// Contains property create, update, delete, and detail errors.
    /// </summary>
    public static class PropertyErrors
    {
        /// <summary>
        /// Error message used when a property cannot be found in the current party scope.
        /// </summary>
        public const string ERROR_PROPERTY_NOT_FOUND = "Property was not found in the current party scope.";

        /// <summary>
        /// Error message used when property-code generation cannot find a free code.
        /// </summary>
        public const string ERROR_PROPERTY_CODE_GENERATION_FAILED = "Could not generate a unique property code.";

        /// <summary>
        /// Error message used when an existing room identifier does not belong to the edited property.
        /// </summary>
        public const string ERROR_PROPERTY_ROOM_NOT_FOUND = "Room was not found in the edited property.";

        /// <summary>
        /// Error message used when an existing charge policy identifier does not belong to the edited property.
        /// </summary>
        public const string ERROR_PROPERTY_CHARGE_POLICY_NOT_FOUND = "Charge policy was not found in the edited property.";

        /// <summary>
        /// Error message used when a property cannot be deleted because it has protected dependency data.
        /// </summary>
        public const string ERROR_PROPERTY_DELETE_BLOCKED = "Property has a contract or active occupancy and cannot be scheduled for deletion.";

        /// <summary>
        /// Error message template used when submitted rooms exceed the create-property limit.
        /// </summary>
        public const string ERROR_GENERATED_UNITS_LIMIT = "Generated units must be less than or equal to {0}.";

        /// <summary>
        /// Error message template used when an update structure has too many rooms.
        /// </summary>
        public const string ERROR_UPDATE_ROOMS_LIMIT = "Updated rooms must be less than or equal to {0}.";

        /// <summary>
        /// Error message used when update-property existing room identifiers are duplicated.
        /// </summary>
        public const string ERROR_PROPERTY_UPDATE_ROOM_IDS_UNIQUE = "Existing room identifiers must be unique within the property update payload.";

        /// <summary>
        /// Error message used when update-property existing charge policy identifiers are duplicated.
        /// </summary>
        public const string ERROR_PROPERTY_UPDATE_POLICY_IDS_UNIQUE = "Existing charge policy identifiers must be unique within the property update payload.";

        /// <summary>
        /// Error message used when update-property package identifiers are duplicated.
        /// </summary>
        public const string ERROR_PROPERTY_UPDATE_PACKAGE_IDS_UNIQUE = "Package ids must be unique within the property update payload.";

        /// <summary>
        /// Error message used when the property update preflight detects conflicting room or section data.
        /// </summary>
        public const string ERROR_PROPERTY_UPDATE_PREFLIGHT_FAILED = "Property update could not be saved because one or more submitted rooms or sections have conflicts.";

        /// <summary>
        /// Error code used when property update preflight fails.
        /// </summary>
        public const string PROPERTY_UPDATE_CONFLICT = "PROPERTY_UPDATE_CONFLICT";
    }

    /// <summary>
    /// Contains room update, delete, and override errors.
    /// </summary>
    public static class RoomErrors
    {
        /// <summary>
        /// Error message used when room-code generation cannot find a free code.
        /// </summary>
        public const string ERROR_ROOM_CODE_GENERATION_FAILED = "Could not generate a unique room code.";

        /// <summary>
        /// Error message used when a room cannot be found in the current party scope.
        /// </summary>
        public const string ERROR_ROOM_NOT_FOUND = "Room was not found in the current party scope.";

        /// <summary>
        /// Error message used when a room with protected dependency data is changed unsafely or deleted.
        /// </summary>
        public const string ERROR_ROOM_MUTATION_BLOCKED = "Room has a contract or active occupancy and cannot be deleted or changed in protected fields.";

        /// <summary>
        /// Error message used when an existing room-level charge policy identifier does not belong to the edited room.
        /// </summary>
        public const string ERROR_ROOM_CHARGE_POLICY_NOT_FOUND = "Room charge policy was not found in the edited room.";

        /// <summary>
        /// Error message used when a room-level package code does not belong to the edited room.
        /// </summary>
        public const string ERROR_ROOM_PACKAGE_NOT_FOUND = "Room package was not found in the edited room.";

        /// <summary>
        /// Error message used when a room override mode is invalid.
        /// </summary>
        public const string ERROR_ROOM_OVERRIDE_MODE_INVALID = "Room override mode is invalid.";

        /// <summary>
        /// Error message used when custom charge policy mode has no custom policies.
        /// </summary>
        public const string ERROR_ROOM_CUSTOM_CHARGE_POLICIES_REQUIRED = "Custom charge policy mode requires at least one charge policy.";

        /// <summary>
        /// Error message used when custom package mode has no custom packages.
        /// </summary>
        public const string ERROR_ROOM_CUSTOM_PACKAGES_REQUIRED = "Custom package mode requires at least one package.";
    }

    /// <summary>
    /// Contains tenant, occupancy, and QR-join errors.
    /// </summary>
    public static class TenantErrors
    {
        /// <summary>
        /// Error message used when tenant role code is invalid.
        /// </summary>
        public const string ERROR_TENANT_ROLE_INVALID = "Tenant role code is invalid.";

        /// <summary>
        /// Error message used when a tenant source is missing.
        /// </summary>
        public const string ERROR_TENANT_SOURCE_REQUIRED = "A tenantId, tenantAccountId, or tenantProfile is required.";

        /// <summary>
        /// Error message used when a tenant party cannot be resolved.
        /// </summary>
        public const string ERROR_TENANT_NOT_FOUND = "Tenant was not found.";

        /// <summary>
        /// Error message used when a user account has several tenant parties.
        /// </summary>
        public const string ERROR_TENANT_ACCOUNT_MULTIPLE_PARTIES = "Tenant account has multiple tenant parties. Select the tenant party explicitly.";

        /// <summary>
        /// Error message used when a room already has a primary tenant.
        /// </summary>
        public const string ERROR_ROOM_PRIMARY_TENANT_EXISTS = "Room already has an active primary tenant.";

        /// <summary>
        /// Error message used when shared-bed capacity is full.
        /// </summary>
        public const string ERROR_ROOM_BED_CAPACITY_EXCEEDED = "Room has no available bed capacity for another primary tenant.";

        /// <summary>
        /// Error message used when a tenant occupancy cannot be found.
        /// </summary>
        public const string ERROR_TENANT_OCCUPANCY_NOT_FOUND = "Tenant occupancy was not found in the current party scope.";

        /// <summary>
        /// Error message used when a tenant join token is invalid or expired.
        /// </summary>
        public const string ERROR_TENANT_JOIN_TOKEN_INVALID = "Tenant join token is invalid or expired.";

        /// <summary>
        /// Error message used when another join operation is already mutating the room.
        /// </summary>
        public const string ERROR_TENANT_JOIN_IN_PROGRESS = "Another tenant join is being processed for this room. Please try again.";

        /// <summary>
        /// Error message used when the tenant already joined or requested to join the room.
        /// </summary>
        public const string ERROR_TENANT_ALREADY_IN_ROOM = "Tenant already has an active or pending occupancy in this room.";

        /// <summary>
        /// Error message used when QR join is confirmed outside tenant party context.
        /// </summary>
        public const string ERROR_TENANT_CONTEXT_REQUIRED = "Current party must be an active tenant context to join a room.";
    }

    /// <summary>
    /// Contains vehicle registration and payer validation errors.
    /// </summary>
    public static class VehicleErrors
    {
        /// <summary>
        /// Error message used when a vehicle is outside the current landlord property scope.
        /// </summary>
        public const string ERROR_VEHICLE_NOT_FOUND = "Vehicle was not found in the current party scope.";

        /// <summary>
        /// Error message used when the requested payer is not an eligible primary tenant signer in the room.
        /// </summary>
        public const string ERROR_VEHICLE_PRIMARY_PAYER_REQUIRED = "Vehicle payer must be the room's active primary tenant with an active contract.";

        /// <summary>
        /// Error message used when object storage cannot complete a vehicle image upload.
        /// </summary>
        public const string ERROR_VEHICLE_IMAGE_UPLOAD_FAILED = "Vehicle images could not be uploaded.";
    }

    /// <summary>
    /// Contains meter validation and invoice synchronization errors.
    /// </summary>
    public static class MeterErrors
    {
        /// <summary>
        /// Error returned when the billing date is missing or invalid.
        /// </summary>
        public const string ERROR_METER_DATE_INVALID = "Billing date is required.";

        /// <summary>
        /// Error returned when the requested meter period is outside the current scope.
        /// </summary>
        public const string ERROR_METER_NOT_FOUND = "Meter period was not found in the current party scope.";

        /// <summary>
        /// Error returned when neither electricity nor water data is supplied.
        /// </summary>
        public const string ERROR_METER_SECTION_REQUIRED = "At least one electricity or water meter value must be supplied.";

        /// <summary>
        /// Error returned when no effective usage-based charge policy exists.
        /// </summary>
        public const string ERROR_METER_POLICY_REQUIRED = "A meter-reading charge policy is required for this room.";

        /// <summary>
        /// Error returned when the current reading is below the previous reading.
        /// </summary>
        public const string ERROR_METER_RANGE = "Current reading must be greater than or equal to the previous reading.";

        /// <summary>
        /// Error returned when the submitted baseline differs from the confirmed reading.
        /// </summary>
        public const string ERROR_METER_PREVIOUS_MISMATCH = "Previous value must match the last confirmed meter value.";

        /// <summary>
        /// Error returned when the room already has a meter record for the month.
        /// </summary>
        public const string ERROR_METER_ALREADY_EXISTS = "A meter record already exists for this room and billing month.";

        /// <summary>
        /// Error returned when invoice payment prevents meter mutation.
        /// </summary>
        public const string ERROR_METER_MUTATION_BLOCKED = "The meter period cannot be changed after invoice payment.";

        /// <summary>
        /// Error returned when another request owns the room meter lock.
        /// </summary>
        public const string ERROR_METER_LOCKED = "Another meter update is in progress. Please try again.";

        /// <summary>
        /// Error returned when one utility contains too many evidence images.
        /// </summary>
        public const string ERROR_METER_EVIDENCE_LIMIT = "Each electricity or water meter value can contain up to five images.";

        /// <summary>
        /// Error returned when selected evidence does not belong to the meter period.
        /// </summary>
        public const string ERROR_METER_EVIDENCE_NOT_FOUND = "One or more selected meter evidence images were not found.";
    }
}
