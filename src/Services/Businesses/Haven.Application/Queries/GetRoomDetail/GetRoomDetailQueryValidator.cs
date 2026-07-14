namespace Haven.Application.Queries.GetRoomDetail;

/// <summary>
/// Validates room detail route input.
/// </summary>
public class GetRoomDetailQueryValidator : AbstractValidator<GetRoomDetailQuery>
{
    /// <summary>
    /// Creates validation rules for the room detail query.
    /// </summary>
    public GetRoomDetailQueryValidator()
    {
        // Room detail reads must always be scoped by a route public identifier.
        RuleFor(x => x.RoomPublicId).RequiredGuid();
    }
}
