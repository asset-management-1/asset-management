namespace Authentication.Infrastructure.Models.Users.Kyc;

/// <summary>
/// Groups the inputs required to create document links for one KYC submission.
/// </summary>
internal sealed class KycDocumentLinkCreationModel
{
    /// <summary>
    /// Gets or sets the tracked parties linked to the user.
    /// </summary>
    public IEnumerable<Party> Parties { get; set; }

    /// <summary>
    /// Gets or sets the front-side or primary document entity.
    /// </summary>
    public Document FrontDocument { get; set; }

    /// <summary>
    /// Gets or sets the optional back-side document entity.
    /// </summary>
    public Document BackDocument { get; set; }

    /// <summary>
    /// Gets or sets the resolved KYC master-data context.
    /// </summary>
    public KycMasterDataContextModel MasterData { get; set; }
}
