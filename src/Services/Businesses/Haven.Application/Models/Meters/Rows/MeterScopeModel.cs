namespace Haven.Application.Models.Meters.Rows;

/// <summary>
/// Contains internal and public property-room identifiers resolved inside landlord scope.
/// </summary>
public sealed class MeterScopeModel
{
    /// <summary>
    /// Gets or sets the internal property identifier used by persistence queries.
    /// </summary>
    public long PropertyId { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe property identifier.
    /// </summary>
    public Guid PropertyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the property display name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the internal room identifier used by persistence queries.
    /// </summary>
    public long UnitId { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomPublicId { get; set; }

    /// <summary>
    /// Gets or sets the room display name.
    /// </summary>
    public string RoomName { get; set; }
}
