namespace Haven.Application.Queries.GetTenantDetail;

/// <summary>
/// Represents a tenant occupancy detail query.
/// </summary>
/// <param name="Id">The occupancy public identifier.</param>
public sealed record GetTenantDetailQuery(Guid Id) : IQuery<ResponseDto<TenantDetailResponseDto>>;
