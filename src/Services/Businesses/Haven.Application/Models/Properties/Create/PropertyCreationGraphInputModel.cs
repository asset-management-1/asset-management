namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Carries validated creation input plus exact lookup values for graph mapping.
/// </summary>
public class PropertyCreationGraphInputModel
{
    /// <summary>
    /// Gets or sets the original property creation request.
    /// </summary>
    public PropertyCreationRequestModel Request { get; set; }

    /// <summary>
    /// Gets or sets the resolved lookup values needed by graph mapping.
    /// </summary>
    public PropertyCreationLookupModel Lookups { get; set; }
}
