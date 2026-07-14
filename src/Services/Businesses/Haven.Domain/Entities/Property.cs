namespace Haven.Domain.Entities;

/// <summary>
/// Represents a landlord-managed property or building container in asset.Properties.
/// </summary>
public class Property : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique property code used internally by Haven.
    /// </summary>
    public string PropertyCode { get; set; }

    /// <summary>
    /// Gets or sets the display name of the property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the master-data identifier for the property type.
    /// </summary>
    public long PropertyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the optional property description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the province identifier.
    /// </summary>
    public long? ProvinceId { get; set; }

    /// <summary>
    /// Gets or sets the district identifier.
    /// </summary>
    public long? DistrictId { get; set; }

    /// <summary>
    /// Gets or sets the ward identifier.
    /// </summary>
    public long? WardId { get; set; }

    /// <summary>
    /// Gets or sets the street-level address.
    /// </summary>
    public string StreetAddress { get; set; }

    /// <summary>
    /// Gets or sets the formatted address shown to users.
    /// </summary>
    public string FormattedAddress { get; set; }

    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the total floor count snapshot.
    /// </summary>
    public int? TotalFloors { get; set; }

    /// <summary>
    /// Gets or sets the total unit count snapshot.
    /// </summary>
    public int? TotalUnits { get; set; }

    /// <summary>
    /// Gets or sets the property status master-data identifier.
    /// </summary>
    public long StatusId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is published publicly.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets or sets when the landlord requested property deletion.
    /// </summary>
    public DateTime? DeleteRequestedAt { get; set; }

    /// <summary>
    /// Gets or sets when the pending property deletion becomes eligible for final cleanup.
    /// </summary>
    public DateTime? DeleteScheduledAt { get; set; }

    /// <summary>
    /// Gets or sets the internal party identifier that requested deletion.
    /// </summary>
    public long? DeleteRequestedByPartyId { get; set; }

    /// <summary>
    /// Gets the units generated under this property.
    /// </summary>
    public ICollection<Unit> Units { get; } = new List<Unit>();

    /// <summary>
    /// Gets the parties linked to this property.
    /// </summary>
    public ICollection<PropertyParty> PropertyParties { get; } = new List<PropertyParty>();

    /// <summary>
    /// Gets the property-level rental charge policies.
    /// </summary>
    public ICollection<RentalChargePolicy> RentalChargePolicies { get; } = new List<RentalChargePolicy>();

    /// <summary>
    /// Gets the common package templates configured for this property.
    /// </summary>
    public ICollection<UnitPackage> UnitPackages { get; } = new List<UnitPackage>();

    /// <summary>
    /// Recomputes structure counters from the currently tracked active unit graph.
    /// </summary>
    public void RecomputeStructureTotals()
    {
        // Total floors and rooms are derived snapshots from the active unit collection.
        TotalFloors = Units
            .Where(unit => unit.FloorNumber.HasValue)
            .Select(unit => unit.FloorNumber.Value)
            .Distinct()
            .Count();
        TotalUnits = Units.Count;
    }
}
