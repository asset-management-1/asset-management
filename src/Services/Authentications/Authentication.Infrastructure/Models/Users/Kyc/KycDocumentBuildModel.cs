namespace Authentication.Infrastructure.Models.Users.Kyc;

/// <summary>
/// Groups the inputs required to build one persisted KYC document row.
/// </summary>
internal sealed class KycDocumentBuildModel
{
    /// <summary>
    /// Gets or sets the form file submitted by the client.
    /// </summary>
    public IFormFile File { get; set; }

    /// <summary>
    /// Gets or sets the private object-storage upload metadata.
    /// </summary>
    public ObjectUploadResponseModel Upload { get; set; }

    /// <summary>
    /// Gets or sets the resolved KYC master-data context.
    /// </summary>
    public KycMasterDataContextModel MasterData { get; set; }
}
