namespace Authentication.Application.Commands.Kyc;

/// <summary>
/// Represents a current-user identity-document KYC submission request.
/// </summary>
public class SubmitKycCommand : ICommand<ResponseDto<KycSubmissionResponseDto>>
{
    /// <summary>
    /// Gets or sets the personal identity document type. Accepted values: <c>Cccd</c>, <c>Passport</c>.
    /// </summary>
    public IdentifierTypeEnum IdentifierType { get; set; }

    /// <summary>
    /// Gets or sets the identity document identifier value.
    /// </summary>
    public string IdentifierValue { get; set; }

    /// <summary>
    /// Gets or sets the full name shown on the document.
    /// </summary>
    public string FullNameOnDocument { get; set; }

    /// <summary>
    /// Gets or sets the date of birth shown on the document.
    /// </summary>
    public DateOnly? DateOfBirthOnDocument { get; set; }

    /// <summary>
    /// Gets or sets the gender shown on the document. Accepted values: <c>Male</c>, <c>Female</c>.
    /// </summary>
    public GenderEnum GenderOnDocument { get; set; }

    /// <summary>
    /// Gets or sets the registered address shown on the document.
    /// </summary>
    public string RegisteredAddress { get; set; }

    /// <summary>
    /// Gets or sets the document issue date.
    /// </summary>
    public DateOnly? IssuedDate { get; set; }

    /// <summary>
    /// Gets or sets the document expiration date.
    /// </summary>
    public DateOnly? ExpiredDate { get; set; }

    /// <summary>
    /// Gets or sets the authority that issued the document.
    /// </summary>
    public string IssuedBy { get; set; }

    /// <summary>
    /// Gets or sets the front-side or primary identity document scan file. Send as a binary multipart file part.
    /// </summary>
    public IFormFile FrontFile { get; set; }

    /// <summary>
    /// Gets or sets the optional back-side identity document scan file. CCCD requires this file; Passport does not.
    /// </summary>
    public IFormFile BackFile { get; set; }
}
