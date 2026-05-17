namespace Authentication.Infrastructure.Models.Users.Vehicles;

/// <summary>
/// Groups uploaded tenant vehicle image metadata by image side.
/// </summary>
internal sealed class VehicleUploadedImagesModel
{
    /// <summary>
    /// Gets or sets the front-side vehicle image upload metadata.
    /// </summary>
    public ObjectUploadResponseModel Front { get; set; }

    /// <summary>
    /// Gets or sets the side vehicle image upload metadata.
    /// </summary>
    public ObjectUploadResponseModel Side { get; set; }

    /// <summary>
    /// Gets uploaded objects that should be cleaned up when persistence fails.
    /// </summary>
    public IReadOnlyCollection<ObjectUploadResponseModel> UploadedObjects => [Front, Side];
}
