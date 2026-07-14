namespace Haven.Application.Queries.GetRoomTenantJoinQr;

/// <summary>
/// Validates room tenant-join QR queries.
/// </summary>
public class GetRoomTenantJoinQrQueryValidator : AbstractValidator<GetRoomTenantJoinQrQuery>
{
    /// <summary>
    /// Creates validation rules for QR generation.
    /// </summary>
    public GetRoomTenantJoinQrQueryValidator()
    {
        // QR generation always starts from a room action, so the route/body room id must be present.
        RuleFor(x => x.RoomPublicId).RequiredGuid();

        // The role is embedded in the QR payload; FE must not generate ambiguous tenant join tokens.
        RuleFor(x => x.RoleCode)
            .Required()
            .MaxLen(100)
            .Must(roleCode => string.Equals(roleCode, TENANT_ROLE_PRIMARY, StringComparison.OrdinalIgnoreCase)
                              || string.Equals(roleCode, TENANT_ROLE_OCCUPANT, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ROLE_INVALID);
    }
}
