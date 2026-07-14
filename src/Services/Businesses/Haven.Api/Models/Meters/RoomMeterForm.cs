namespace Haven.Api.Models.Meters;

/// <summary>
/// Represents multipart electricity and water values submitted for one room meter period.
/// </summary>
public class RoomMeterForm
{
    /// <summary>
    /// Gets or sets the meter date bound from the multipart form.
    /// </summary>
    public DateTime BillingDate { get; set; }

    /// <summary>
    /// Gets or sets the first-period electricity baseline.
    /// </summary>
    public decimal? ElectricPrevious { get; set; }

    /// <summary>
    /// Gets or sets the current electricity meter value.
    /// </summary>
    public decimal? ElectricCurrent { get; set; }

    /// <summary>
    /// Gets or sets the electricity price selected for this period.
    /// </summary>
    public decimal? ElectricPrice { get; set; }

    /// <summary>
    /// Gets or sets electricity images appended to the selected meter period.
    /// </summary>
    public List<IFormFile> ElectricImages { get; set; } = [];

    /// <summary>
    /// Gets or sets the first-period water baseline.
    /// </summary>
    public decimal? WaterPrevious { get; set; }

    /// <summary>
    /// Gets or sets the current water meter value.
    /// </summary>
    public decimal? WaterCurrent { get; set; }

    /// <summary>
    /// Gets or sets the water price selected for this period.
    /// </summary>
    public decimal? WaterPrice { get; set; }

    /// <summary>
    /// Gets or sets water images appended to the selected meter period.
    /// </summary>
    public List<IFormFile> WaterImages { get; set; } = [];
}
