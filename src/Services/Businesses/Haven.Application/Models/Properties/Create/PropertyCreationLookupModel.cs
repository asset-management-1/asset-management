namespace Haven.Application.Models.Properties.Create;

/// <summary>
/// Carries the exact resolved lookup values needed to build a property creation graph.
/// </summary>
public class PropertyCreationLookupModel
{
    /// <summary>
    /// Gets or sets the resolved property type identifier.
    /// </summary>
    public long PropertyTypeId { get; set; }

    /// <summary>
    /// Gets or sets the resolved draft property status identifier.
    /// </summary>
    public long PropertyStatusId { get; set; }

    /// <summary>
    /// Gets or sets the resolved available unit status identifier.
    /// </summary>
    public long UnitStatusId { get; set; }

    /// <summary>
    /// Gets or sets the resolved active common status identifier.
    /// </summary>
    public long ActiveStatusId { get; set; }

    /// <summary>
    /// Gets or sets the resolved landlord relationship type identifier.
    /// </summary>
    public long LandlordRelationshipTypeId { get; set; }

    /// <summary>
    /// Gets or sets the resolved default no-furniture package type.
    /// </summary>
    public MasterDataValueModel NoFurniturePackageType { get; set; }

    /// <summary>
    /// Gets or sets the resolved custom package type when custom packages are requested.
    /// </summary>
    public MasterDataValueModel CustomPackageType { get; set; }

    /// <summary>
    /// Gets or sets the resolved active package status.
    /// </summary>
    public MasterDataValueModel PackageStatus { get; set; }

    /// <summary>
    /// Gets or sets resolved unit type identifiers keyed by request code.
    /// </summary>
    public IReadOnlyDictionary<string, long> UnitTypeIds { get; set; } = new Dictionary<string, long>();

    /// <summary>
    /// Gets or sets resolved rental mode identifiers keyed by request code.
    /// </summary>
    public IReadOnlyDictionary<string, long> RentalModeIds { get; set; } = new Dictionary<string, long>();

    /// <summary>
    /// Gets or sets resolved invoice line types keyed by request code.
    /// </summary>
    public IReadOnlyDictionary<string, MasterDataValueModel> ChargeTypes { get; set; } =
        new Dictionary<string, MasterDataValueModel>();

    /// <summary>
    /// Gets or sets resolved vehicle type identifiers keyed by request code.
    /// </summary>
    public IReadOnlyDictionary<string, long> VehicleTypeIds { get; set; } = new Dictionary<string, long>();
}
