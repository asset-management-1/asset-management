namespace Haven.Application.Commands.CreateTenant;

/// <summary>
/// Represents a landlord request to add a tenant or member to a room.
/// </summary>
public class CreateTenantCommand : ICommand<ResponseDto<TenantDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the frontend-safe room identifier.
    /// </summary>
    public Guid RoomId { get; set; }

    /// <summary>
    /// Gets or sets the role code: PRIMARY or OCCUPANT.
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Gets or sets an existing tenant party identifier.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets an existing tenant account identifier.
    /// </summary>
    public Guid? TenantAccountId { get; set; }

    /// <summary>
    /// Gets or sets inline tenant profile data when the tenant has no account.
    /// </summary>
    public TenantProfileRequestDto TenantProfile { get; set; }

    /// <summary>
    /// Gets or sets the contract or occupancy start date.
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Gets or sets the contract end date.
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Gets or sets the contract rent amount.
    /// </summary>
    public decimal? ContractRentAmount { get; set; }

    /// <summary>
    /// Gets or sets the contract deposit amount.
    /// </summary>
    public decimal? DepositAmount { get; set; }
}
