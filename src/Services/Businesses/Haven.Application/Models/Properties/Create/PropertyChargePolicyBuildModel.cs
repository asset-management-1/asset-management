namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Represents the resolved input used to map one rental charge policy entity.
/// </summary>
public class PropertyChargePolicyBuildModel
{
    /// <summary>
    /// Gets or sets the parent property entity.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the original charge policy request.
    /// </summary>
    public CreatePropertyChargePolicyRequestDto Policy { get; set; }

    /// <summary>
    /// Gets or sets the resolved invoice line type value.
    /// </summary>
    public MasterDataValueModel ChargeType { get; set; }

    /// <summary>
    /// Gets or sets the optional resolved vehicle type identifier.
    /// </summary>
    public long? VehicleTypeId { get; set; }

    /// <summary>
    /// Gets or sets the resolved active status identifier.
    /// </summary>
    public long ActiveStatusId { get; set; }
}

