namespace Authentication.Infrastructure.Models.Users.Kyc;

/// <summary>
/// Groups uploaded private KYC document metadata by document slot.
/// </summary>
internal sealed class KycUploadedDocumentsModel
{
    /// <summary>
    /// Gets or sets the front-side or primary private document upload metadata.
    /// </summary>
    public ObjectUploadResponseModel Front { get; set; }

    /// <summary>
    /// Gets or sets the optional back-side private document upload metadata.
    /// </summary>
    public ObjectUploadResponseModel Back { get; set; }

    /// <summary>
    /// Gets uploaded objects that should be cleaned up when persistence fails.
    /// </summary>
    public IReadOnlyCollection<ObjectUploadResponseModel> UploadedObjects => [Front, Back];
}
