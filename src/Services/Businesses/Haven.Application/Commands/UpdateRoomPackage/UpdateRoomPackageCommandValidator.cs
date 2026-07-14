namespace Haven.Application.Commands.UpdateRoomPackage;

/// <summary>
/// Validates partial room package updates.
/// </summary>
public sealed class UpdateRoomPackageCommandValidator : AbstractValidator<UpdateRoomPackageCommand>
{
    /// <summary>
    /// Creates identity and optional-field validation rules.
    /// </summary>
    public UpdateRoomPackageCommandValidator()
    {
        // Both ids are required because a common package must be interpreted in the selected room context.
        RuleFor(x => x.Id).RequiredGuid();
        RuleFor(x => x.RoomId).RequiredGuid();

        // Submitted scalar fields are validated while omitted values preserve persisted data.
        RuleFor(x => x.Name).Required().MaxLen(255).When(x => x.Name is not null);
        RuleFor(x => x.PriceAdjustment).GreaterOrEqualZeroWhenPresent();

        // Null preserves items, an empty list clears them, and a populated list must contain valid unique labels.
        RuleFor(x => x.Items).MaxCount(MAX_PACKAGE_ITEMS);
        RuleForEach(x => x.Items).ChildRules(item =>
            item.RuleFor(x => x.Name).Required().MaxLen(255));
        RuleFor(x => x.Items)
            .Must(items => items is null
                           || items.Select(item => item.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == items.Count)
            .WithMessage(ApplicationErrorConstants.RoomPackageErrors.ERROR_ROOM_PACKAGE_ITEMS_DUPLICATED);
    }
}
