namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Represents the entity graph staged for a new landlord property setup.
/// </summary>
public class PropertyCreationGraphModel
{
    /// <summary>
    /// Gets or sets the property entity.
    /// </summary>
    public Property Property { get; set; }

    /// <summary>
    /// Gets or sets generated unit entities.
    /// </summary>
    public IReadOnlyList<Unit> Units { get; set; } = [];

    /// <summary>
    /// Gets or sets generated unit packages.
    /// </summary>
    public IReadOnlyList<UnitPackage> UnitPackages { get; set; } = [];

    /// <summary>
    /// Gets or sets generated package item entities.
    /// </summary>
    public IReadOnlyList<UnitPackageItem> UnitPackageItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the current party's property relationship.
    /// </summary>
    public PropertyParty PropertyParty { get; set; }

    /// <summary>
    /// Gets or sets property-level rental charge policies.
    /// </summary>
    public IReadOnlyList<RentalChargePolicy> ChargePolicies { get; set; } = [];
}

