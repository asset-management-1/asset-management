namespace Haven.Application.Commands.RestorePropertyDelete;

/// <summary>
/// Represents a request to restore a pending property delete.
/// </summary>
/// <param name="PropertyPublicId">The property public identifier from the route.</param>
public sealed record RestorePropertyDeleteCommand(Guid PropertyPublicId) : ICommand<ResponseDto<OperationStatusResponseDto>>;
