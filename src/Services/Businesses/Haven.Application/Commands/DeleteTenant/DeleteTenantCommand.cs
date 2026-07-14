namespace Haven.Application.Commands.DeleteTenant;

/// <summary>
/// Represents a request to move one tenant occupancy out.
/// </summary>
/// <param name="Id">The occupancy public identifier.</param>
public sealed record DeleteTenantCommand(Guid Id) : ICommand<ResponseDto<OperationStatusResponseDto>>;
