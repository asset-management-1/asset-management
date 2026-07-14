namespace Haven.Application.Commands.DeleteRoom;

/// <summary>
/// Validates room delete route input.
/// </summary>
public class DeleteRoomCommandValidator : AbstractValidator<DeleteRoomCommand>
{
    /// <summary>
    /// Creates validation rules for room delete.
    /// </summary>
    public DeleteRoomCommandValidator()
    {
        // Room delete is route-scoped and cannot proceed without a public room identifier.
        RuleFor(x => x.RoomPublicId).RequiredGuid();
    }
}
