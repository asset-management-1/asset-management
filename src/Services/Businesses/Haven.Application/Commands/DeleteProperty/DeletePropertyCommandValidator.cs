namespace Haven.Application.Commands.DeleteProperty;

/// <summary>
/// Validates the property delete request.
/// </summary>
public class DeletePropertyCommandValidator : AbstractValidator<DeletePropertyCommand>
{
    /// <summary>
    /// Creates validation rules for property delete.
    /// </summary>
    public DeletePropertyCommandValidator()
    {
        // Delete is route-scoped and cannot proceed without a public property identifier.
        RuleFor(x => x.PropertyPublicId).RequiredGuid();
    }
}
