namespace Haven.Infrastructure.Models.Tenants;

/// <summary>
/// Carries the exact master-data values needed by tenant write flows.
/// </summary>
public class TenantMasterDataContextModel
{
    /// <summary>
    /// Gets or sets the tenant party type value.
    /// </summary>
    public MasterDataValueModel TenantPartyType { get; set; }

    /// <summary>
    /// Gets or sets the active party status value.
    /// </summary>
    public MasterDataValueModel PartyActiveStatus { get; set; }

    /// <summary>
    /// Gets or sets the rental contract type value.
    /// </summary>
    public MasterDataValueModel ContractTypeRental { get; set; }

    /// <summary>
    /// Gets or sets the offline-upload contract source value.
    /// </summary>
    public MasterDataValueModel ContractSourceOfflineUpload { get; set; }

    /// <summary>
    /// Gets or sets the active contract status value.
    /// </summary>
    public MasterDataValueModel ContractStatusActive { get; set; }

    /// <summary>
    /// Gets or sets the active occupancy status value.
    /// </summary>
    public MasterDataValueModel OccupancyStatusActive { get; set; }

    /// <summary>
    /// Gets or sets the pending occupancy status value.
    /// </summary>
    public MasterDataValueModel OccupancyStatusPending { get; set; }

    /// <summary>
    /// Gets or sets the moved-out occupancy status value.
    /// </summary>
    public MasterDataValueModel OccupancyStatusMovedOut { get; set; }

    /// <summary>
    /// Gets or sets the shared-bed rental mode value.
    /// </summary>
    public MasterDataValueModel SharedBedRentalMode { get; set; }

    /// <summary>
    /// Gets or sets the occupied unit status value.
    /// </summary>
    public MasterDataValueModel UnitStatusOccupied { get; set; }

    /// <summary>
    /// Gets or sets the available unit status value.
    /// </summary>
    public MasterDataValueModel UnitStatusAvailable { get; set; }
}
