namespace Authentication.Application.Dtos.Users.Kyc;

/// <summary>
/// Represents the current user's identity-document summary without exposing private file paths.
/// </summary>
public class KycSummaryResponseDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the user has submitted identity data.
    /// </summary>
    public bool IsSubmitted { get; set; }

    /// <summary>
    /// Gets or sets the manual KYC review status.
    /// </summary>
    public KycStatusEnum? Status { get; set; }

    /// <summary>
    /// Gets or sets the submitted identity document type.
    /// </summary>
    public IdentifierTypeEnum? IdentifierType { get; set; }

    /// <summary>
    /// Gets or sets the submitted identity document type display name.
    /// </summary>
    public string IdentifierTypeDisplayName { get; set; }

    /// <summary>
    /// Gets or sets the masked identifier value, exposing only safe trailing characters.
    /// </summary>
    public string MaskedIdentifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a front-side or primary identity scan exists.
    /// </summary>
    public bool HasFrontFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a back-side identity scan exists.
    /// </summary>
    public bool HasBackFile { get; set; }
}
