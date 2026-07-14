namespace Haven.Application.Commands.CreateRoomPackage;

/// <summary>
/// Validates room package creation requests.
/// </summary>
public sealed class CreateRoomPackageCommandValidator : AbstractValidator<CreateRoomPackageCommand>
{
    /// <summary>
    /// Creates package identity, price, and item validation rules.
    /// </summary>
    public CreateRoomPackageCommandValidator()
    {
        // A package must target one real room and provide the two values shown by the package form.
        RuleFor(x => x.RoomId).RequiredGuid();
        RuleFor(x => x.Name).Required().MaxLen(255);
        RuleFor(x => x.PriceAdjustment).GreaterOrEqualZero();

        // Item labels follow the database limit and remain unique within the submitted package.
        RuleFor(x => x.Items).NotNull();
        RuleFor(x => x.Items).MaxCount(MAX_PACKAGE_ITEMS);
        RuleForEach(x => x.Items).ChildRules(item =>
            item.RuleFor(x => x.Name).Required().MaxLen(255));
        RuleFor(x => x.Items)
            .Must(items => items is null
                           || items.Select(item => item.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == items.Count)
            .WithMessage(ApplicationErrorConstants.RoomPackageErrors.ERROR_ROOM_PACKAGE_ITEMS_DUPLICATED);
    }
}
