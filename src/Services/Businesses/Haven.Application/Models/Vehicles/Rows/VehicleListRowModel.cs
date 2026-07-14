namespace Haven.Application.Models.Vehicles.Rows;

/// <summary>
/// Represents one room vehicle management row returned by Dapper.
/// </summary>
public sealed class VehicleListRowModel
{
    /// <summary>
    /// Gets or sets the frontend-safe vehicle identifier.
    /// </summary>
    public Guid VehiclePublicId { get; set; }

    /// <summary>
    /// Gets or sets the frontend-safe payer tenant identifier.
    /// </summary>
    public Guid PayerTenantPublicId { get; set; }

    /// <summary>
    /// Gets or sets the payer tenant display name.
    /// </summary>
    public string PayerTenantName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type master-data code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the vehicle type display name.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the vehicle display name.
    /// </summary>
    public string VehicleName { get; set; }

    /// <summary>
    /// Gets or sets the optional license plate.
    /// </summary>
    public string LicensePlate { get; set; }

}
