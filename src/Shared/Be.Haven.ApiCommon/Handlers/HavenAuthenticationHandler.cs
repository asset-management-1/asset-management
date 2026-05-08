namespace Be.Haven.ApiCommon.Handlers;

/// <summary>
/// Custom authentication handler for Haven access tokens issued by the authentication service.
/// </summary>
public class HavenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AuthenticationTokenValidationOptions _authOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="HavenAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">Provides scheme options for the current authentication handler instance.</param>
    /// <param name="logger">Creates the logger used by the base authentication handler.</param>
    /// <param name="encoder">Encodes outbound content when required by the base authentication handler.</param>
    /// <param name="authOptions">Provides the shared Haven JWT validation settings.</param>
    public HavenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<AuthenticationTokenValidationOptions> authOptions)
        : base(options, logger, encoder)
    {
        _authOptions = authOptions.Value;
    }

    /// <summary>
    /// Authenticates the current request by reading the bearer token, validating it, and normalizing only the claims Haven needs.
    /// </summary>
    /// <returns>An authentication result that indicates success, failure, or no result for anonymous requests.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (Request.Path.StartsWithSegments(HEALTH))
            {
                Logger.LogDebug(HavenAuthenticationLogs.LOG_AUTH_SKIPPED_HEALTH, Request.Path.Value, Request.Method);
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Authentication is enforced only for endpoints that carry authorization metadata
            // and are not explicitly opened with AllowAnonymous.
            var endpointRequiresAuthorization = RequiresAuthorization();
            if (!TryReadBearerToken(out var token))
            {
                if (!endpointRequiresAuthorization)
                {
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                Logger.LogDebug(HavenAuthenticationLogs.LOG_AUTH_FAILED, Request.Path.Value, Request.Method);
                return Task.FromResult(AuthenticateResult.Fail(AUTHENTICATION_FAIL));
            }

            Logger.LogInformation(HavenAuthenticationLogs.LOG_AUTH_STARTED, Request.Path.Value, Request.Method);

            // Token validation stays inside the handler so downstream code only sees an authenticated principal.
            var principal = Validate(token);
            Logger.LogInformation(HavenAuthenticationLogs.LOG_AUTH_SUCCEEDED, principal.FindFirstValue(ClaimTypes.NameIdentifier));

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (SecurityTokenExpiredException ex)
        {
            Logger.LogWarning(ex, HavenAuthenticationLogs.LOG_AUTH_REJECTED_EXPIRED);
            return Task.FromResult(AuthenticateResult.Fail(TOKEN_EXPIRED));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, HavenAuthenticationLogs.LOG_AUTH_REJECTED, ex.GetType().Name, ex.Message);
            var failureMessage = string.Format(INVALID_TOKEN, ex.Message);
            return Task.FromResult(AuthenticateResult.Fail(failureMessage));
        }
    }

    /// <summary>
    /// Writes the standardized unauthorized response when authentication fails for a protected request.
    /// </summary>
    /// <param name="properties">Additional authentication properties supplied by ASP.NET Core.</param>
    /// <returns>A task that writes the unauthorized response body.</returns>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Logger.LogInformation(HavenAuthenticationLogs.LOG_CHALLENGE_401);

        return WriteErrorAsync(
            statusCode: StatusCodes.Status401Unauthorized,
            code: UNAUTHORIZED,
            defaultMessage: AUTHENTICATION_FAIL);
    }

    /// <summary>
    /// Writes the standardized forbidden response when the current identity lacks permission for the endpoint.
    /// </summary>
    /// <param name="properties">Additional authentication properties supplied by ASP.NET Core.</param>
    /// <returns>A task that writes the forbidden response body.</returns>
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Logger.LogInformation(HavenAuthenticationLogs.LOG_FORBIDDEN_403);

        return WriteErrorAsync(
            statusCode: StatusCodes.Status403Forbidden,
            code: FORBIDDEN,
            defaultMessage: MISSING_PERMISSION);
    }

    /// <summary>
    /// Determines whether the resolved endpoint requires authorization.
    /// </summary>
    /// <returns><c>true</c> when the endpoint requires authorization; otherwise <c>false</c>.</returns>
    private bool RequiresAuthorization()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return false;
        }

        return endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null;
    }

    /// <summary>
    /// Reads the bearer token from the authorization header and ensures the request uses the expected header format.
    /// </summary>
    /// <param name="token">The resolved raw JWT token when the header is valid.</param>
    /// <returns><c>true</c> when a bearer token is present and well formed; otherwise <c>false</c>.</returns>
    private bool TryReadBearerToken(out string token)
    {
        token = string.Empty;

        if (!Request.Headers.TryGetValue(AUTHORIZATION, out var headerValue)
            || !AuthenticationHeaderValue.TryParse(headerValue, out var authorizationHeader)
            || !string.Equals(authorizationHeader.Scheme, BEARER, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(authorizationHeader.Parameter))
        {
            return false;
        }

        token = authorizationHeader.Parameter;
        return true;
    }

    /// <summary>
    /// Validates the bearer token using the configured Haven JWT settings and returns a minimally normalized principal.
    /// </summary>
    /// <param name="token">The raw JWT token string from the authorization header.</param>
    /// <returns>The validated claims principal.</returns>
    private ClaimsPrincipal Validate(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _authOptions.Issuer,
            ValidateAudience = true,
            ValidAudiences = _authOptions.Audiences,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        // Haven controls token issuance, so validation is strict and normalization stays minimal.
        var principal = handler.ValidateToken(token, parameters, out _);
        return NormalizePrincipal(principal);
    }

    /// <summary>
    /// Validates the required Haven identity claims and applies only the fallback claim aliases the solution still depends on.
    /// </summary>
    /// <param name="principal">The validated principal returned by the JWT token handler.</param>
    /// <returns>The validated principal with any required fallback claims added.</returns>
    private static ClaimsPrincipal NormalizePrincipal(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            throw new SecurityTokenException(AUTHENTICATION_FAIL);
        }

        var currentUserId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? identity.FindFirst(TokenClaimTypes.SUBJECT)?.Value;
        if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out _))
        {
            throw new SecurityTokenException(AUTHENTICATION_FAIL);
        }

        // Keep downstream code on a stable claim shape even when older tokens still rely on fallback names.
        TokenHelper.EnsureClaim(identity, ClaimTypes.NameIdentifier, currentUserId);

        var currentUserName = identity.FindFirst(ClaimTypes.Name)?.Value
                              ?? identity.FindFirst(TokenClaimTypes.NAME)?.Value;
        if (!string.IsNullOrWhiteSpace(currentUserName))
        {
            TokenHelper.EnsureClaim(identity, ClaimTypes.Name, currentUserName);
        }

        var currentUserEmail = identity.FindFirst(ClaimTypes.Email)?.Value
                               ?? identity.FindFirst(TokenClaimTypes.EMAIL)?.Value;
        if (!string.IsNullOrWhiteSpace(currentUserEmail))
        {
            TokenHelper.EnsureClaim(identity, ClaimTypes.Email, currentUserEmail);
        }

        foreach (var roleClaim in identity.FindAll(TokenClaimTypes.ROLE))
        {
            TokenHelper.EnsureClaim(identity, ClaimTypes.Role, roleClaim.Value);
        }

        foreach (var rolesClaim in identity.FindAll(TokenClaimTypes.ROLES))
        {
            foreach (var roleValue in rolesClaim.Value.Split(
                         [',', ';', ' '],
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                TokenHelper.EnsureClaim(identity, ClaimTypes.Role, roleValue);
            }
        }

        return principal;
    }

    /// <summary>
    /// Writes the standardized JSON error response used for unauthorized and forbidden authentication outcomes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code written to the response.</param>
    /// <param name="code">The application error code written to the response body.</param>
    /// <param name="defaultMessage">The fallback error message used when no authentication failure message is available.</param>
    /// <returns>A task that writes the response body.</returns>
    private Task WriteErrorAsync(int statusCode, string code, string defaultMessage)
    {
        var failure = Context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Failure;
        var message = failure?.Message ?? defaultMessage;

        Logger.LogInformation(HavenAuthenticationLogs.LOG_WRITE_ERROR_PAYLOAD, statusCode, code, message);

        var version = Context.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        string versionData = null;
        if (version is not null)
        {
            versionData = $"{version.MajorVersion}.{version.MinorVersion ?? 0}";
        }

        var activity = Activity.Current;
        Context.Items.TryGetValue(X_CORRELATION_ID, out var correlationObject);
        Context.Items.TryGetValue(REQUEST_TIMESTAMP, out var requestTimestampObject);

        var response = new ResponseDto<string>
        {
            Meta = new MetaDetailDto
            {
                RequestId = Context.TraceIdentifier,
                CorrelationId = correlationObject?.ToString(),
                SpanId = activity?.SpanId.ToString(),
                TraceId = activity?.TraceId.ToString(),
                Version = versionData,
                RequestTimestamp = requestTimestampObject as DateTime?,
                ResponseTimestamp = DateTime.UtcNow
            },
            Error = new ErrorDto
            {
                Code = code,
                StatusCode = statusCode,
                Message = message
            }
        };

        Response.StatusCode = statusCode;
        Response.ContentType = TEXT_JSON;

        // The handler writes the same response envelope shape as the rest of the API error pipeline.
        return Response.WriteAsJsonAsync(response);
    }
}
