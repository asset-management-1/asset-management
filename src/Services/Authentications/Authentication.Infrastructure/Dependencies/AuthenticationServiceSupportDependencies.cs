namespace Authentication.Infrastructure.Dependencies;

/// <summary>
/// Groups support dependencies used by local authentication service flows.
/// </summary>
public class AuthenticationServiceSupportDependencies
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceSupportDependencies"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used for transactional writes.</param>
    /// <param name="emailService">The shared email sender used for OTP and security notifications.</param>
    /// <param name="passwordHasher">The password hasher used for local credential verification and hashing.</param>
    /// <param name="cachingService">The cache service used for auth reset markers.</param>
    /// <param name="authOptions">The configured JWT and refresh-token options.</param>
    /// <param name="emailOptions">The configured email options used by security notifications.</param>
    public AuthenticationServiceSupportDependencies(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPasswordHasher<User> passwordHasher,
        ICachingService cachingService,
        IOptions<AuthOptions> authOptions,
        IOptions<EmailOptions> emailOptions)
    {
        // Expose the grouped dependencies without hiding behavior behind another workflow service.
        UnitOfWork = unitOfWork;
        EmailService = emailService;
        PasswordHasher = passwordHasher;
        CachingService = cachingService;
        AuthOptions = authOptions.Value;
        EmailOptions = emailOptions.Value;
    }

    /// <summary>
    /// Gets the unit of work used for transactional writes.
    /// </summary>
    public IUnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Gets the shared email sender.
    /// </summary>
    public IEmailService EmailService { get; }

    /// <summary>
    /// Gets the password hasher.
    /// </summary>
    public IPasswordHasher<User> PasswordHasher { get; }

    /// <summary>
    /// Gets the cache service used for auth reset markers.
    /// </summary>
    public ICachingService CachingService { get; }

    /// <summary>
    /// Gets the configured JWT and refresh-token options.
    /// </summary>
    public AuthOptions AuthOptions { get; }

    /// <summary>
    /// Gets the configured email options.
    /// </summary>
    public EmailOptions EmailOptions { get; }
}
