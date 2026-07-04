namespace Haven.Application.Queries.GetPropertyDetail;

/// <summary>
/// Represents a landlord-scoped query for one building detail.
/// </summary>
/// <param name="PropertyPublicId">The property public identifier from the route.</param>
public sealed record GetPropertyDetailQuery(Guid PropertyPublicId) : IQuery<ResponseDto<PropertyDetailResponseDto>>;
