namespace Haven.Application.Commands.DeleteProperty;

/// <summary>
/// Represents a request to soft-delete a landlord property/building.
/// </summary>
/// <param name="PropertyPublicId">The property public identifier from the route.</param>
public sealed record DeletePropertyCommand(Guid PropertyPublicId) : ICommand<ResponseDto<OperationStatusResponseDto>>;
