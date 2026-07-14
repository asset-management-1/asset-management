namespace Haven.Application.Queries.GetRoomPackageDetail;

/// <summary>
/// Validates room package detail identifiers.
/// </summary>
public sealed class GetRoomPackageDetailQueryValidator : AbstractValidator<GetRoomPackageDetailQuery>
{
    /// <summary>
    /// Creates frontend-safe identifier validation rules.
    /// </summary>
    public GetRoomPackageDetailQueryValidator()
    {
        // The package must be resolved inside the submitted room rather than as an unscoped global identifier.
        RuleFor(x => x.PackagePublicId).RequiredGuid();
        RuleFor(x => x.RoomPublicId).RequiredGuid();
    }
}
