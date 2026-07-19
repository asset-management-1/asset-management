namespace Authentication.Infrastructure.Dependencies;

/// <summary>
/// Groups repositories used by authentication workflow services.
/// </summary>
public class AuthenticationRepositoryDependencies
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AuthenticationRepositoryDependencies"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="refreshTokenRepository">The refresh-token repository.</param>
    /// <param name="masterDataValueRepository">The master-data value repository.</param>
    /// <param name="partyRepository">The party repository.</param>
    /// <param name="userPartyRepository">The user-party repository.</param>
    /// <param name="externalLoginRepository">The external-login repository.</param>
    public AuthenticationRepositoryDependencies(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IMasterDataValueRepository masterDataValueRepository,
        IPartyRepository partyRepository,
        IUserPartyRepository userPartyRepository,
        IExternalLoginRepository externalLoginRepository)
    {
        UserRepository = userRepository;
        RefreshTokenRepository = refreshTokenRepository;
        MasterDataValueRepository = masterDataValueRepository;
        PartyRepository = partyRepository;
        UserPartyRepository = userPartyRepository;
        ExternalLoginRepository = externalLoginRepository;
    }

    /// <summary>
    /// Gets the user repository.
    /// </summary>
    public IUserRepository UserRepository { get; }

    /// <summary>
    /// Gets the refresh-token repository.
    /// </summary>
    public IRefreshTokenRepository RefreshTokenRepository { get; }

    /// <summary>
    /// Gets the master-data value repository.
    /// </summary>
    public IMasterDataValueRepository MasterDataValueRepository { get; }

    /// <summary>
    /// Gets the party repository.
    /// </summary>
    public IPartyRepository PartyRepository { get; }

    /// <summary>
    /// Gets the user-party repository.
    /// </summary>
    public IUserPartyRepository UserPartyRepository { get; }

    /// <summary>
    /// Gets the external-login repository.
    /// </summary>
    public IExternalLoginRepository ExternalLoginRepository { get; }
}
