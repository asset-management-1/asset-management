namespace Authentication.Application.Commands.Kyc;

/// <summary>
/// Validates current-user identity-document KYC submissions.
/// </summary>
public class SubmitKycCommandValidator : AbstractValidator<SubmitKycCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitKycCommandValidator"/> class.
    /// </summary>
    public SubmitKycCommandValidator()
    {
        // Validate shared document fields first; Mapster owns enum-to-master-data normalization after validation.
        RuleFor(x => x.IdentifierType).IsInEnum();

        RuleFor(x => x.IdentifierValue).Required().MaximumLength(150);

        RuleFor(x => x.FullNameOnDocument).Required().MaximumLength(255);

        RuleFor(x => x.GenderOnDocument).IsInEnum();

        RuleFor(x => x.RegisteredAddress).Required().MaximumLength(500);

        RuleFor(x => x.IssuedBy).Required().MaximumLength(150);

        RuleFor(x => x.DateOfBirthOnDocument).NotNull();

        RuleFor(x => x.DateOfBirthOnDocument.Value)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirthOnDocument.HasValue);

        RuleFor(x => x.IssuedDate).NotNull();

        RuleFor(x => x.IssuedDate.Value)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.IssuedDate.HasValue);

        RuleFor(x => x.ExpiredDate).NotNull();

        RuleFor(x => x.ExpiredDate.Value)
            .GreaterThanOrEqualTo(x => x.IssuedDate.Value)
            .When(x => x.ExpiredDate.HasValue && x.IssuedDate.HasValue);

        RuleFor(x => x.FrontFile)
            .Must(x => x is not null && x.Length > 0)
            .WithMessage(KYC_FILE_REQUIRED_MESSAGE);

        // CCCD requires a back-side scan while Passport only uses the primary/front scan.
        RuleFor(x => x.BackFile)
            .Must(x => x is not null && x.Length > 0)
            .When(x => x.IdentifierType == IdentifierTypeEnum.Cccd)
            .WithMessage(KYC_FILE_REQUIRED_MESSAGE);
    }
}
