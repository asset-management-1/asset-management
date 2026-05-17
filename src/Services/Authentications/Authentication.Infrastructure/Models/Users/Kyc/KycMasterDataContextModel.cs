namespace Authentication.Infrastructure.Models.Users.Kyc;

/// <summary>
/// Groups master-data values required to persist one KYC submission.
/// </summary>
internal sealed class KycMasterDataContextModel
{
    /// <summary>
    /// Gets or sets the selected personal identity document type value.
    /// </summary>
    public MasterDataValue IdentifierType { get; set; }

    /// <summary>
    /// Gets or sets the personal identity document type values supported by this KYC flow.
    /// </summary>
    public IReadOnlyCollection<MasterDataValue> SupportedIdentifierTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets the document gender value.
    /// </summary>
    public MasterDataValue Gender { get; set; }

    /// <summary>
    /// Gets or sets the national-id document type value.
    /// </summary>
    public MasterDataValue DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the R2 storage provider value.
    /// </summary>
    public MasterDataValue StorageProvider { get; set; }

    /// <summary>
    /// Gets or sets the uploaded document status value.
    /// </summary>
    public MasterDataValue DocumentStatus { get; set; }

    /// <summary>
    /// Gets or sets the party entity type value.
    /// </summary>
    public MasterDataValue EntityType { get; set; }

    /// <summary>
    /// Gets or sets the front-side or primary document-link type value.
    /// </summary>
    public MasterDataValue FrontLinkType { get; set; }

    /// <summary>
    /// Gets or sets the optional back-side document-link type value.
    /// </summary>
    public MasterDataValue BackLinkType { get; set; }

    /// <summary>
    /// Gets or sets the active document-link status value.
    /// </summary>
    public MasterDataValue LinkStatus { get; set; }

    /// <summary>
    /// Gets or sets the pending manual-review status value for KYC submissions.
    /// </summary>
    public MasterDataValue KycStatus { get; set; }

    /// <summary>
    /// Gets or sets the rejected manual-review status value used to decide re-upload eligibility.
    /// </summary>
    public MasterDataValue RejectedKycStatus { get; set; }
}
