namespace Authentication.Application.Commands.Kyc;

/// <summary>
/// Validates current-user identity-document KYC submissions.
/// </summary>
public class SubmitKycCommandValidator : AbstractValidator<SubmitKycCommand>
{
    /// <summary>
    /// Creates validation rules for the identity document fields and scan files.
    /// </summary>
    public SubmitKycCommandValidator()
    {
        // Identifier values are validated before Mapster converts enums into master-data codes.
        RuleFor(x => x.IdentifierType).ValidEnum();

        RuleFor(x => x.IdentifierValue).Required().MaxLen(150);

        RuleFor(x => x.FullNameOnDocument).Required().MaxLen(255);

        RuleFor(x => x.GenderOnDocument).ValidEnum();

        // Issuer and address text are bounded because they are copied directly from OCR/user input.
        RuleFor(x => x.RegisteredAddress).Required().MaxLen(500);

        RuleFor(x => x.IssuedBy).Required().MaxLen(150);

        // Document dates must describe a real document lifecycle, not a future-issued document.
        RuleFor(x => x.DateOfBirthOnDocument).Required();

        RuleFor(x => x.DateOfBirthOnDocument.Value)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirthOnDocument.HasValue);

        RuleFor(x => x.IssuedDate).Required();

        RuleFor(x => x.IssuedDate.Value)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.IssuedDate.HasValue);

        RuleFor(x => x.ExpiredDate).Required();

        RuleFor(x => x.ExpiredDate.Value)
            .GreaterThanOrEqualTo(x => x.IssuedDate.Value)
            .When(x => x.ExpiredDate.HasValue && x.IssuedDate.HasValue);

        // Every KYC document needs a primary/front scan with actual file content.
        RuleFor(x => x.FrontFile)
            .Must(x => x is not null && x.Length > 0)
            .WithMessage(ApplicationErrorConstants.KycErrors.KYC_FILE_REQUIRED_MESSAGE)
            .DependentRules(() => RuleFor(x => x.FrontFile).OptionalImageFile());

        // CCCD requires a back-side scan while Passport only uses the primary/front scan.
        RuleFor(x => x.BackFile)
            .Must(x => x is not null && x.Length > 0)
            .When(x => x.IdentifierType == IdentifierTypeEnum.Cccd)
            .WithMessage(ApplicationErrorConstants.KycErrors.KYC_FILE_REQUIRED_MESSAGE)
            .DependentRules(() => RuleFor(x => x.BackFile).OptionalImageFile());
    }
}
