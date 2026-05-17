namespace Authentication.Infrastructure.Models.Users.Kyc;

/// <summary>
/// Groups the inputs required to decide whether a user may submit KYC.
/// </summary>
internal sealed class KycSubmissionGuardModel
{
    /// <summary>
    /// Gets or sets the current user's public identifier used for safe logging.
    /// </summary>
    public Guid CurrentUserPublicId { get; set; }

    /// <summary>
    /// Gets or sets the tracked parties linked to the user.
    /// </summary>
    public IReadOnlyCollection<Party> Parties { get; set; }

    /// <summary>
    /// Gets or sets the supported personal identifier type master-data value ids.
    /// </summary>
    public IReadOnlyCollection<long> IdentifierTypeIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the rejected KYC status master-data value id.
    /// </summary>
    public long RejectedStatusId { get; set; }
}
