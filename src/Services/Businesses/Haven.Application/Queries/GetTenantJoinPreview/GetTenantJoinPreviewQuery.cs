namespace Haven.Application.Queries.GetTenantJoinPreview;

/// <summary>
/// Represents a tenant join preview query.
/// </summary>
/// <param name="Token">The QR token.</param>
public sealed record GetTenantJoinPreviewQuery(string Token) : IQuery<ResponseDto<TenantJoinPreviewResponseDto>>;
