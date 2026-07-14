namespace Haven.Application.Commands.DeleteRoomPackage;

/// <summary>
/// Validates room package delete identifiers.
/// </summary>
public sealed class DeleteRoomPackageCommandValidator : AbstractValidator<DeleteRoomPackageCommand>
{
    /// <summary>
    /// Creates frontend-safe identifier validation rules.
    /// </summary>
    public DeleteRoomPackageCommandValidator()
    {
        // Both identifiers are required because deletion resolves effective package ownership inside one room context.
        RuleFor(x => x.PackagePublicId).RequiredGuid();
        RuleFor(x => x.RoomPublicId).RequiredGuid();
    }
}
