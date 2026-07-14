namespace Haven.Application.Models.Meters.Detail;

/// <summary>
/// Carries the selected room, calendar period, and landlord scope into a meter detail read.
/// </summary>
public sealed class MeterPeriodRequestModel
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the selected calendar month.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the selected calendar year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the current landlord party scope.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }
}
