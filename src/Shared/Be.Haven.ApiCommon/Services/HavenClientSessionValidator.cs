namespace Be.Haven.ApiCommon.Services;

/// <summary>
/// Validates Haven access tokens against active refresh-token client-session state.
/// </summary>
public class HavenClientSessionValidator : IClientSessionValidator
{
    private readonly IDapperService _dapperService;
    private readonly ILogger<HavenClientSessionValidator> _logger;

    /// <summary>
    /// Creates the client-session validator with Dapper source-of-truth access.
    /// </summary>
    /// <param name="dapperService">The Dapper service used for session lookups.</param>
    /// <param name="logger">The validator logger.</param>
    public HavenClientSessionValidator(
        IDapperService dapperService,
        ILogger<HavenClientSessionValidator> logger)
    {
        _dapperService = dapperService;
        _logger = logger;
    }

    /// <summary>
    /// Determines whether the supplied session is active and belongs to the supplied user.
    /// </summary>
    /// <param name="userPublicId">The token user's public identifier.</param>
    /// <param name="sessionPublicId">The token session's public identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the validation lookup.</param>
    /// <returns><c>true</c> when the session is active for the user; otherwise <c>false</c>.</returns>
    public async Task<bool> IsSessionActiveAsync(
        Guid userPublicId,
        Guid sessionPublicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Session validation is a minimal source-of-truth read used by both public APIs.
            var readModel = await _dapperService.QueryFirstOrDefaultAsync<ClientSessionValidationReadModel>(
                HavenClientSessionConstants.GET_ACTIVE_SESSION_BY_USER_AND_PUBLIC_ID_QUERY,
                new
                {
                    UserPublicId = userPublicId,
                    SessionPublicId = sessionPublicId
                },
                DapperCommandOptionsHelper.CreateText(cancellationToken));

            return readModel is not null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, HavenAuthenticationLogs.LOG_CLIENT_SESSION_DB_LOOKUP_FAILED, userPublicId, sessionPublicId);
            throw new HttpStatusCodeException(
                AUTH_STATE_UNAVAILABLE,
                SERVICE_UNAVAILABLE,
                StatusCodes.Status503ServiceUnavailable);
        }
    }
}
