namespace Authentication.Domain.Enums;

/// <summary>
/// Represents supported manual KYC review status values returned by authentication APIs.
/// </summary>
public enum KycStatusEnum
{
    /// <summary>
    /// KYC submission is waiting for manual review.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// KYC submission has been approved.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// KYC submission has been rejected.
    /// </summary>
    Rejected = 3
}
