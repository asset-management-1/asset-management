namespace Haven.Application.Models.Rooms.Rows;

/// <summary>
/// Row model for one effective room charge policy.
/// </summary>
public class RoomChargePolicyRowModel
{
    /// <summary>
    /// Gets or sets the charge policy public identifier.
    /// </summary>
    public Guid PolicyPublicId { get; set; }

    /// <summary>
    /// Gets or sets the invoice line type code.
    /// </summary>
    public string ChargeTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the invoice line type display name.
    /// </summary>
    public string ChargeTypeName { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle type code.
    /// </summary>
    public string VehicleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the optional vehicle type display name.
    /// </summary>
    public string VehicleTypeName { get; set; }

    /// <summary>
    /// Gets or sets the charge display name.
    /// </summary>
    public string ChargeName { get; set; }

    /// <summary>
    /// Gets or sets whether the charge is usage based.
    /// </summary>
    public bool IsUsageBased { get; set; }

    /// <summary>
    /// Gets or sets the charge amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the policy status code.
    /// </summary>
    public string StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the policy status display name.
    /// </summary>
    public string StatusName { get; set; }
}
