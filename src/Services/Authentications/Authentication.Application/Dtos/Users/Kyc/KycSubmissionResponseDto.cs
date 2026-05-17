namespace Authentication.Application.Dtos.Users.Kyc;

/// <summary>
/// Represents the result of storing an identity-document submission.
/// </summary>
public class KycSubmissionResponseDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the identity submission was stored.
    /// </summary>
    public bool IsSubmitted { get; set; }

    /// <summary>
    /// Gets or sets the manual KYC review status.
    /// </summary>
    public KycStatusEnum Status { get; set; }

    /// <summary>
    /// Gets or sets the result message.
    /// </summary>
    public string Message { get; set; }
}
