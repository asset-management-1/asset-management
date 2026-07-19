namespace Authentication.Application.Dtos.Users.Kyc;

/// <summary>
/// Represents the current-user identity-document KYC submission payload.
/// </summary>
public class SubmitKycRequestDto
{
    /// <summary>
    /// Gets or sets the normalised personal identity document type code.
    /// </summary>
    public string IdentifierType { get; set; }

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
    /// Gets or sets the gender code shown on the document.
    /// </summary>
    public string GenderOnDocument { get; set; }

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
    /// Gets or sets the front-side or primary identity document scan file.
    /// </summary>
    public IFormFile FrontFile { get; set; }

    /// <summary>
    /// Gets or sets the optional back-side identity document scan file.
    /// </summary>
    public IFormFile BackFile { get; set; }
}
