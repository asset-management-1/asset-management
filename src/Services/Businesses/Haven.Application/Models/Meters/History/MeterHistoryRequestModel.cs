namespace Haven.Application.Models.Meters.History;

/// <summary>
/// Carries the room, calendar year, and landlord scope into a bounded meter history read.
/// </summary>
public sealed class MeterHistoryRequestModel
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the calendar year whose months are returned.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the current landlord party scope.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }
}
