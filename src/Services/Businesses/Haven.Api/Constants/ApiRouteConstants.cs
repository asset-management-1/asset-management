namespace Haven.Api.Constants;

/// <summary>
/// Contains constant string values representing Haven API route segments.
/// </summary>
public static class ApiRouteConstants
{
    /// <summary>
    /// Route parameter name for a property public identifier.
    /// </summary>
    public const string PROPERTY_PUBLIC_ID_ROUTE_PARAMETER = "property-public-id";

    /// <summary>
    /// Route segment for one property by public identifier.
    /// </summary>
    public const string PROPERTY_BY_PUBLIC_ID = "{" + PROPERTY_PUBLIC_ID_ROUTE_PARAMETER + "}";

    /// <summary>
    /// Route segment for restoring a pending property delete request.
    /// </summary>
    public const string PROPERTY_RESTORE_DELETE = PROPERTY_BY_PUBLIC_ID + "/restore-delete";

    /// <summary>
    /// Route parameter name for a room public identifier.
    /// </summary>
    public const string ROOM_ID_ROUTE_PARAMETER = "room-id";

    /// <summary>
    /// Route segment for one room by public identifier.
    /// </summary>
    public const string ROOM_BY_ID = "{" + ROOM_ID_ROUTE_PARAMETER + "}";

    /// <summary>
    /// Absolute route segment for generating a room tenant-join QR token.
    /// </summary>
    public const string ROOM_TENANT_JOIN_QR = "~/v{version:apiVersion}/rooms/" + ROOM_BY_ID + "/tenant-join-qr";

    /// <summary>
    /// Absolute route root for vehicles managed inside one room.
    /// </summary>
    public const string ROOM_VEHICLES_ROOT = "~/v{version:apiVersion}/rooms/" + ROOM_BY_ID + "/vehicles";

    /// <summary>
    /// Route parameter name for a vehicle public identifier.
    /// </summary>
    public const string VEHICLE_ID_ROUTE_PARAMETER = "vehicle-id";

    /// <summary>
    /// Route segment for one vehicle inside its owning room.
    /// </summary>
    public const string ROOM_VEHICLE_BY_ID = "{" + VEHICLE_ID_ROUTE_PARAMETER + "}";

    /// <summary>
    /// Absolute route root for monthly meter records managed inside one room.
    /// </summary>
    public const string ROOM_METERS_ROOT = "~/v{version:apiVersion}/rooms/" + ROOM_BY_ID + "/meters";

    /// <summary>
    /// Route segment for one selected meter period.
    /// </summary>
    public const string ROOM_METERS_PERIOD = "period";

    /// <summary>
    /// Route parameter name for a public response identifier.
    /// </summary>
    public const string ID_ROUTE_PARAMETER = "id";

    /// <summary>
    /// Route segment for one resource by frontend-safe identifier.
    /// </summary>
    public const string BY_ID = "{" + ID_ROUTE_PARAMETER + "}";

    /// <summary>
    /// Route parameter name for a tenant join QR token.
    /// </summary>
    public const string TENANT_JOIN_TOKEN_ROUTE_PARAMETER = "token";

    /// <summary>
    /// Absolute route root for tenant join QR actions handled by the tenant controller.
    /// </summary>
    public const string TENANT_JOIN_ROOT = "~/v{version:apiVersion}/tenant-joins";

    /// <summary>
    /// Absolute route segment for previewing one tenant join token.
    /// </summary>
    public const string TENANT_JOIN_BY_TOKEN = TENANT_JOIN_ROOT + "/{" + TENANT_JOIN_TOKEN_ROUTE_PARAMETER + "}";
}
