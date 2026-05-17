namespace Authentication.Infrastructure.Dependencies;

/// <summary>
/// Groups authentication repositories shared by the three workflow services.
/// </summary>
public class AuthenticationRepositoryDependencies
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationRepositoryDependencies"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="refreshTokenRepository">The refresh-token repository.</param>
    /// <param name="masterDataValueRepository">The master-data value repository.</param>
    /// <param name="partyRepository">The party repository.</param>
    /// <param name="userPartyRepository">The user-party repository.</param>
    public AuthenticationRepositoryDependencies(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IMasterDataValueRepository masterDataValueRepository,
        IPartyRepository partyRepository,
        IUserPartyRepository userPartyRepository)
    {
        UserRepository = userRepository;
        RefreshTokenRepository = refreshTokenRepository;
        MasterDataValueRepository = masterDataValueRepository;
        PartyRepository = partyRepository;
        UserPartyRepository = userPartyRepository;
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

}
