namespace Be.Haven.ApiCommon.Handlers;

/// <summary>
/// Custom authentication handler for Haven access tokens issued by the authentication service.
/// </summary>
public class HavenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AuthenticationTokenValidationOptions _authOptions;
    private readonly IAuthResetValidator _authResetValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="HavenAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">Provides scheme options for the current authentication handler instance.</param>
    /// <param name="logger">Creates the logger used by the base authentication handler.</param>
    /// <param name="encoder">Encodes outbound content when required by the base authentication handler.</param>
    /// <param name="authOptions">Provides the shared Haven JWT validation settings.</param>
    /// <param name="authResetValidator">Validates access-token issue time against the latest user auth reset marker.</param>
    public HavenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<AuthenticationTokenValidationOptions> authOptions,
        IAuthResetValidator authResetValidator)
        : base(options, logger, encoder)
    {
        // Keep shared JWT settings and reset validation dependency local to the handler instance.
        _authOptions = authOptions.Value;
        _authResetValidator = authResetValidator;
    }

    /// <summary>
    /// Authenticates the current request by reading the bearer token, validating it, and normalizing only the claims Haven needs.
    /// </summary>
    /// <returns>An authentication result that indicates success, failure, or no result for anonymous requests.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            // Health checks do not participate in auth so infrastructure probes stay cheap and anonymous.
            if (Request.Path.StartsWithSegments(HEALTH))
            {
                Logger.LogDebug(HavenAuthenticationLogs.LOG_AUTH_SKIPPED_HEALTH, Request.Path.Value, Request.Method);
                return AuthenticateResult.NoResult();
            }

            // Authentication is enforced only for endpoints that carry authorization metadata
            // and are not explicitly opened with AllowAnonymous.
            var endpointRequiresAuthorization = RequiresAuthorization();
            if (!TryReadBearerToken(out var token))
            {
                // Anonymous endpoints may continue; protected endpoints fail so the challenge writes the 401 envelope.
                if (!endpointRequiresAuthorization)
                {
                    return AuthenticateResult.NoResult();
                }

                Logger.LogDebug(HavenAuthenticationLogs.LOG_AUTH_FAILED, Request.Path.Value, Request.Method);
                return AuthenticateResult.Fail(AUTHENTICATION_FAIL);
            }

            Logger.LogInformation(HavenAuthenticationLogs.LOG_AUTH_STARTED, Request.Path.Value, Request.Method);

            // Token validation stays inside the handler so downstream code only sees an authenticated principal.
            var principal = Validate(token);

            // Auth reset validation rejects old access tokens after password reset or logout-all.
            await ValidateAuthResetAsync(principal, Context.RequestAborted);
            Logger.LogInformation(HavenAuthenticationLogs.LOG_AUTH_SUCCEEDED, principal.FindFirstValue(ClaimTypes.NameIdentifier));

            // Successful authentication returns the normalized principal under the configured scheme.
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (SecurityTokenExpiredException ex)
        {
            // Expired tokens keep a specific client-safe failure message.
            Logger.LogWarning(ex, HavenAuthenticationLogs.LOG_AUTH_REJECTED_EXPIRED);
            return AuthenticateResult.Fail(TOKEN_EXPIRED);
        }
        catch (HttpStatusCodeException ex) when (ex.StatusCode == StatusCodes.Status503ServiceUnavailable)
        {
            // Dependency failures are preserved so the challenge can return 503 instead of a generic 401.
            Logger.LogWarning(ex, HavenAuthenticationLogs.LOG_AUTH_REJECTED, ex.GetType().Name, ex.Message);
            return AuthenticateResult.Fail(ex);
        }
        catch (Exception ex)
        {
            // Validation details stay in logs; clients only receive the generic invalid-token message.
            Logger.LogWarning(ex, HavenAuthenticationLogs.LOG_AUTH_REJECTED, ex.GetType().Name, ex.Message);
            return AuthenticateResult.Fail(INVALID_TOKEN);
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
    /// Rejects tokens issued before the user's latest authentication reset marker.
    /// </summary>
    /// <param name="principal">The validated principal returned by the JWT token handler.</param>
    /// <param name="cancellationToken">The token used to cancel cache or database reset-state lookups.</param>
    /// <returns>A task that completes when the token reset-state check passes.</returns>
    private async Task ValidateAuthResetAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        // The handler already normalized NameIdentifier, so reset validation can trust this claim shape.
        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userPublicId))
        {
            throw new SecurityTokenException(AUTHENTICATION_FAIL);
        }

        var issuedAtClaim = principal.FindFirstValue(TokenClaimTypes.HAVEN_ISSUED_AT_MS);
        if (!long.TryParse(issuedAtClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var issuedAtMs))
        {
            throw new SecurityTokenException(INVALID_TOKEN);
        }

        if (await _authResetValidator.IsTokenValidAsync(userPublicId, issuedAtMs, cancellationToken))
        {
            return;
        }

        Logger.LogWarning(HavenAuthenticationLogs.LOG_AUTH_REJECTED_RESET, userPublicId);
        throw new SecurityTokenException(INVALID_TOKEN);
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
        // Preserve dependency status failures while keeping normal auth failures on the supplied status/code.
        var failure = Context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Failure;
        var message = failure is HttpStatusCodeException statusCodeException
            ? statusCodeException.Message
            : failure?.Message ?? defaultMessage;
        if (failure is HttpStatusCodeException statusCodeExceptionForPayload)
        {
            statusCode = statusCodeExceptionForPayload.StatusCode;
            code = string.IsNullOrWhiteSpace(statusCodeExceptionForPayload.ErrorCode)
                ? code
                : statusCodeExceptionForPayload.ErrorCode;
        }

        Logger.LogInformation(HavenAuthenticationLogs.LOG_WRITE_ERROR_PAYLOAD, statusCode, code, message);

        // Rebuild the same metadata envelope used by the centralized API error middleware.
        var version = Context.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        string versionData = null;
        if (version is not null)
        {
            versionData = $"{version.MajorVersion}.{version.MinorVersion ?? 0}";
        }

        var activity = Activity.Current;
        Context.Items.TryGetValue(X_CORRELATION_ID, out var correlationObject);
        Context.Items.TryGetValue(REQUEST_TIMESTAMP, out var requestTimestampObject);

        // Authentication handlers bypass MVC middleware, so they must write the response DTO directly.
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
