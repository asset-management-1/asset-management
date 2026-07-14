namespace Haven.Application.Queries.GetVehicles;

/// <summary>
/// Validates the room scope used to load its bounded vehicle collection.
/// </summary>
public sealed class GetVehiclesQueryValidator : AbstractValidator<GetVehiclesQuery>
{
    /// <summary>
    /// Creates the required room-scope rule.
    /// </summary>
    public GetVehiclesQueryValidator()
    {
        // Vehicle management is available only from a concrete room context.
        RuleFor(x => x.RoomId).RequiredGuid();

    }
}
