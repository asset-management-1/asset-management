namespace Authentication.Application.Commands.UpdateUserInfo;

/// <summary>
/// Validates current-user profile update requests.
/// </summary>
public class UpdateUserInfoCommandValidator : AbstractValidator<UpdateUserInfoCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserInfoCommandValidator"/> class.
    /// </summary>
    public UpdateUserInfoCommandValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.AvatarFile)
            .Must(x => x is null || x.Length > 0)
            .WithMessage(AVATAR_FILE_EMPTY_MESSAGE);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .When(x => x.Gender.HasValue);
    }
}
