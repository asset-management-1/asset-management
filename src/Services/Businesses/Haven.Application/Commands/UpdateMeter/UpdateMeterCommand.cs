namespace Haven.Application.Commands.UpdateMeter;

/// <summary>
/// Represents submitted changes for an existing room meter period.
/// </summary>
public class UpdateMeterCommand : ICommand<ResponseDto<MeterPeriodDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the room identifier supplied by the API route.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the validated meter date.
    /// </summary>
    public DateTime BillingDate { get; set; }

    /// <summary>
    /// Gets or sets the manual electricity baseline for the first recorded period.
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
    public IReadOnlyList<IFormFile> ElectricImages { get; set; } = [];

    /// <summary>
    /// Gets or sets selected electricity evidence image identifiers to remove.
    /// </summary>
    public IReadOnlyList<Guid> DeletedElectricImageIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the manual water baseline for the first recorded period.
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
    public IReadOnlyList<IFormFile> WaterImages { get; set; } = [];

    /// <summary>
    /// Gets or sets selected water evidence image identifiers to remove.
    /// </summary>
    public IReadOnlyList<Guid> DeletedWaterImageIds { get; set; } = [];
}
