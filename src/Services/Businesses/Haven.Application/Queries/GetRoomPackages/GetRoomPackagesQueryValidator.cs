namespace Haven.Application.Queries.GetRoomPackages;

/// <summary>
/// Validates room package list queries.
/// </summary>
public sealed class GetRoomPackagesQueryValidator : AbstractValidator<GetRoomPackagesQuery>
{
    /// <summary>
    /// Creates the required room identifier rule.
    /// </summary>
    public GetRoomPackagesQueryValidator()
    {
        // Effective package lookup requires one frontend-safe room context.
        RuleFor(x => x.RoomId).RequiredGuid();
    }
}
