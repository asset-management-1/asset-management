namespace Haven.Application.Queries.GetTenantJoinPreview;

/// <summary>
/// Validates tenant join preview route input.
/// </summary>
public class GetTenantJoinPreviewQueryValidator : AbstractValidator<GetTenantJoinPreviewQuery>
{
    /// <summary>
    /// Creates validation rules for tenant join preview queries.
    /// </summary>
    public GetTenantJoinPreviewQueryValidator()
    {
        // Preview loads only from a backend-issued short-lived QR token.
        RuleFor(x => x.Token).Required().MaxLen(TENANT_JOIN_TOKEN_MAX_LENGTH);
    }
}




