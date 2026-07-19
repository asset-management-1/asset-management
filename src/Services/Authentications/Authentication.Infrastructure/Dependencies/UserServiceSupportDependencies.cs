namespace Authentication.Infrastructure.Dependencies;

/// <summary>
/// Groups external support dependencies used by current-user profile, context, and KYC flows.
/// </summary>
public class UserServiceSupportDependencies
{
    /// <summary>
    /// Initialises a new instance of the <see cref="UserServiceSupportDependencies"/> class.
    /// </summary>
    /// <param name="objectStorageService">The private object storage service used for file uploads.</param>
    /// <param name="imageOptimizationService">The shared image processor used before image objects are uploaded.</param>
    /// <param name="externalAuthenticationOptions">The configured external identity-provider options.</param>
    /// <param name="r2StorageOptions">The configured Cloudflare R2 storage options.</param>
    public UserServiceSupportDependencies(
        IObjectStorageService objectStorageService,
        IImageOptimizationService imageOptimizationService,
        IOptions<ExternalAuthenticationOptions> externalAuthenticationOptions,
        IOptions<R2StorageOptions> r2StorageOptions)
    {
        ObjectStorageService = objectStorageService;
        ImageOptimizationService = imageOptimizationService;
        ExternalAuthenticationOptions = externalAuthenticationOptions.Value;
        R2StorageOptions = r2StorageOptions.Value;
    }

    /// <summary>
    /// Gets the private object storage service used for file uploads.
    /// </summary>
    public IObjectStorageService ObjectStorageService { get; }

    /// <summary>
    /// Gets the shared image processor used for avatar and KYC scan uploads.
    /// </summary>
    public IImageOptimizationService ImageOptimizationService { get; }

    /// <summary>
    /// Gets the configured external identity-provider options.
    /// </summary>
    public ExternalAuthenticationOptions ExternalAuthenticationOptions { get; }

    /// <summary>
    /// Gets the configured Cloudflare R2 storage options.
    /// </summary>
    public R2StorageOptions R2StorageOptions { get; }
}
