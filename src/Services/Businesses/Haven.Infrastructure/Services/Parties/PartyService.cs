namespace Haven.Infrastructure.Services.Parties;

/// <summary>
/// Resolves the authenticated user's active party context.
/// </summary>
public class PartyService : IPartyService
{
    private readonly IAuthService _authService;
    private readonly IPartyRepository _partyRepository;
    private readonly ILogger<PartyService> _logger;

    /// <summary>
    /// Creates the party resolver.
    /// </summary>
    /// <param name="authService">The authenticated principal accessor.</param>
    /// <param name="partyRepository">The party repository.</param>
    /// <param name="logger">The service logger.</param>
    public PartyService(
        IAuthService authService,
        IPartyRepository partyRepository,
        ILogger<PartyService> logger)
    {
        _authService = authService;
        _partyRepository = partyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current party context or throws when no authenticated party can be used.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The current party context.</returns>
    public async Task<CurrentPartyContextModel> GetCurrentPartyAsync(CancellationToken cancellationToken = default)
    {
        // Current-party reads require an authenticated user before the database lookup is meaningful.
        var userPublicId = _authService.UserId();

        if (!userPublicId.HasValue)
        {
            _logger.LogWarning(InfrastructureLogConstants.PartyLogs.CURRENT_PARTY_USER_MISSING);

            throw new HttpStatusCodeException(
                ApplicationErrorConstants.ContextErrors.ERROR_AUTHENTICATED_USER_REQUIRED,
                UNAUTHORIZED,
                StatusCodes.Status401Unauthorized);
        }

        // Current party is small and security-sensitive enough to read directly from the source of truth.
        var currentParty = await _partyRepository.GetCurrentPartyByUserPublicIdAsync(
            userPublicId.Value,
            cancellationToken);

        if (currentParty is null)
        {
            _logger.LogWarning(
                InfrastructureLogConstants.PartyLogs.CURRENT_PARTY_MISSING,
                userPublicId.Value);

            throw new ApiException(
                ApplicationErrorConstants.ContextErrors.ERROR_CURRENT_PARTY_REQUIRED,
                FORBIDDEN,
                StatusCodes.Status403Forbidden);
        }

        _logger.LogInformation(
            InfrastructureLogConstants.PartyLogs.CURRENT_PARTY_LOADED,
            currentParty.PartyPublicId,
            userPublicId.Value);

        return currentParty;
    }

    /// <summary>
    /// Gets the current party and ensures it is an active landlord context.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the lookup.</param>
    /// <returns>The current active landlord party context.</returns>
    public async Task<CurrentPartyContextModel> GetCurrentLandlordAsync(CancellationToken cancellationToken = default)
    {
        // Load the active party context first, then enforce the landlord-only service boundary.
        var currentParty = await GetCurrentPartyAsync(cancellationToken);

        // A non-landlord current party is a valid account context but invalid for Haven landlord workflows.
        if (!string.Equals(currentParty.PartyTypeCode, MASTER_CODE_PARTY_TYPE_LANDLORD, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(currentParty.StatusCode, MASTER_CODE_ACTIVE, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                InfrastructureLogConstants.PartyLogs.LANDLORD_CONTEXT_REJECTED,
                currentParty.PartyPublicId,
                currentParty.PartyTypeCode,
                currentParty.StatusCode);

            throw new ApiException(
                ApplicationErrorConstants.ContextErrors.ERROR_LANDLORD_CONTEXT_REQUIRED,
                FORBIDDEN,
                StatusCodes.Status403Forbidden);
        }

        return currentParty;
    }
}
