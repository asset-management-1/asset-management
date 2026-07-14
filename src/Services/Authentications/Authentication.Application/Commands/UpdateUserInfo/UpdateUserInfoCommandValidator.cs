namespace Authentication.Application.Commands.UpdateUserInfo;

/// <summary>
/// Validates current-user profile update requests.
/// </summary>
public class UpdateUserInfoCommandValidator : AbstractValidator<UpdateUserInfoCommand>
{
    /// <summary>
    /// Creates validation rules for optional current-user profile edits.
    /// </summary>
    public UpdateUserInfoCommandValidator()
    {
        // Optional profile text fields are bounded only when supplied.
        RuleFor(x => x.FullName)
            .MaxLen(255)
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleFor(x => x.PhoneNumber)
            .MaxLen(50)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        // Avatar metadata is screened here; Core decodes the actual content before storage accepts it.
        RuleFor(x => x.AvatarFile).OptionalImageFile();

        // Birth date cannot be in the future, and gender must remain inside the API enum contract.
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .ValidEnum()
            .When(x => x.Gender.HasValue);
    }
}
