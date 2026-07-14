namespace Haven.Application.Dtos.Properties.Detail;

/// <summary>
/// Represents the whole-building rental block on the building detail screen.
/// </summary>
public class PropertyWholeBuildingRentalResponseDto
{
    /// <summary>
    /// Gets or sets the active whole-building contract when one exists.
    /// </summary>
    public PropertyWholeBuildingRentalContractResponseDto Contract { get; set; }
}
