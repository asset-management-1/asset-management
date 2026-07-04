namespace Haven.Api.Constants;

/// <summary>
/// Contains constant string values representing API route segments used in controllers.
/// </summary>
public static class ControllerRouteConstants
{
    /// <summary>
    /// Route parameter name for a property public identifier.
    /// </summary>
    public const string PROPERTY_PUBLIC_ID_ROUTE_PARAMETER = "property-public-id";

    /// <summary>
    /// Route segment for one property by public identifier.
    /// </summary>
    public const string PROPERTY_BY_PUBLIC_ID = "{" + PROPERTY_PUBLIC_ID_ROUTE_PARAMETER + "}";
}
