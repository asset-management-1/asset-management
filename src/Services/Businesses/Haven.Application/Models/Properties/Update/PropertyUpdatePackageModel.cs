namespace Haven.Application.Models.Properties.Update;

/// <summary>
/// Represents one package template prepared for synchronization.
/// </summary>
public class PropertyUpdatePackageModel
{
    /// <summary>
    /// Gets or sets the edited package template payload.
    /// </summary>
    public UpdatePropertyPackageRequestDto Package { get; set; }
}
