namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Carries resolved values needed to create a unit package entity.
/// </summary>
public class PropertyUnitPackageBuildModel
{
    /// <summary>
    /// Gets or sets the unit that owns the package.
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// Gets or sets the resolved package type value.
    /// </summary>
    public MasterDataValueModel PackageType { get; set; }

    /// <summary>
    /// Gets or sets the resolved package status value.
    /// </summary>
    public MasterDataValueModel Status { get; set; }

    /// <summary>
    /// Gets or sets the package code unique within the unit.
    /// </summary>
    public string PackageCode { get; set; }

    /// <summary>
    /// Gets or sets the package display name.
    /// </summary>
    public string PackageName { get; set; }

    /// <summary>
    /// Gets or sets the optional package description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the amount added to the room base rent.
    /// </summary>
    public decimal PriceAdjustment { get; set; }
}

