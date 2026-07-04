namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Represents the resolved input used to map a property-party relationship.
/// </summary>
public class PropertyPartyBuildModel
{
    /// <summary>
    /// Gets or sets the parent property entity.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets the current landlord party context.
    /// </summary>
    public CurrentPartyContextModel CurrentParty { get; set; }

    /// <summary>
    /// Gets or sets the resolved relationship type identifier.
    /// </summary>
    public long RelationshipTypeId { get; set; }
}

