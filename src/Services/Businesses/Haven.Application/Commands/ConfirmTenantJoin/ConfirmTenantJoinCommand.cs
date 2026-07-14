namespace Haven.Application.Commands.ConfirmTenantJoin;

/// <summary>
/// Represents a tenant request to confirm a QR room join.
/// </summary>
public class ConfirmTenantJoinCommand : ICommand<ResponseDto<TenantDetailResponseDto>>
{
    /// <summary>
    /// Gets or sets the QR token being confirmed.
    /// </summary>
    public string Token { get; set; }
}
