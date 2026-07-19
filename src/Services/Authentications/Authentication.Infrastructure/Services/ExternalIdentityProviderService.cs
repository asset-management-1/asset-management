namespace Authentication.Infrastructure.Services;

/// <summary>
/// Retrieves trusted Google and Facebook identities after validating the mobile credential contract.
/// </summary>
public sealed class ExternalIdentityProviderService : IExternalIdentityProviderService
{
    private readonly ExternalAuthenticationOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalIdentityProviderService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _metadataManagers;

    /// <summary>
    /// Creates the provider service and caches Google metadata managers for the singleton lifetime.
    /// </summary>
    /// <param name="options">The validated provider options.</param>
    /// <param name="httpClientFactory">The factory for outbound Meta Graph clients.</param>
    /// <param name="logger">The provider-validation logger.</param>
    public ExternalIdentityProviderService(
        IOptions<ExternalAuthenticationOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<ExternalIdentityProviderService> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _metadataManagers = _options.Providers
            .Where(x => x.Name == ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE)
            .ToDictionary(
                x => x.Name,
                CreateMetadataManager,
                StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a normalised trusted identity profile after validating a supported provider credential.
    /// </summary>
    /// <param name="provider">The configured provider name.</param>
    /// <param name="externalToken">The Google ID token or Facebook User Access Token.</param>
    /// <param name="cancellationToken">The token used to cancel provider validation.</param>
    /// <returns>The trusted external identity profile.</returns>
    public Task<ExternalIdentityProfileResponseDto> GetValidatedIdentityAsync(
        string provider,
        string externalToken,
        CancellationToken cancellationToken = default)
    {
        // Reject empty credentials before selecting or calling a provider boundary.
        if (string.IsNullOrWhiteSpace(externalToken))
        {
            throw InvalidToken();
        }

        // Dispatch only to the two mobile-token contracts supported by this release.
        return provider.ToLowerInvariant() switch
        {
            ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE => GetGoogleIdentityAsync(externalToken, cancellationToken),
            ApplicationConstants.EXTERNAL_PROVIDER_FACEBOOK => GetFacebookIdentityAsync(externalToken, cancellationToken),
            _ => throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_PROVIDER_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_INVALID)
        };
    }

    /// <summary>
    /// Gets a trusted Google identity after validating the ID token against cached OIDC metadata.
    /// </summary>
    /// <param name="token">The Google ID token.</param>
    /// <param name="cancellationToken">The token used to cancel metadata retrieval.</param>
    /// <returns>The trusted Google identity profile.</returns>
    private async Task<ExternalIdentityProfileResponseDto> GetGoogleIdentityAsync(
        string token,
        CancellationToken cancellationToken)
    {
        var options = GetProvider(ApplicationConstants.EXTERNAL_PROVIDER_GOOGLE);
        try
        {
            // Step 1: Resolve cached signing metadata and validate signature, issuer, audience, and lifetime together.
            var configuration = await _metadataManagers[options.Name].GetConfigurationAsync(cancellationToken);
            var validationResult = await _tokenHandler.ValidateTokenAsync(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = configuration.SigningKeys,
                    ValidateIssuer = true,
                    ValidIssuers = options.ValidIssuers,
                    ValidateAudience = true,
                    ValidAudiences = options.Audiences,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                });
            if (!validationResult.IsValid || validationResult.ClaimsIdentity is null)
            {
                // Keep provider exception detail in structured logs while returning one stable credential error.
                _logger.LogWarning(
                    validationResult.Exception,
                    InfrastructureLogConstants.ExternalProviderLogs.TOKEN_VALIDATION_FAILED,
                    options.Name);
                throw InvalidToken();
            }

            var principal = new ClaimsPrincipal(validationResult.ClaimsIdentity);

            // Step 2: Accept required identity claims only after cryptographic validation succeeds.
            var providerUserId = principal.FindFirstValue(TokenClaimTypes.SUBJECT)
                                 ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(TokenClaimTypes.EMAIL)
                        ?? principal.FindFirstValue(ClaimTypes.Email);
            var verified = bool.TryParse(
                principal.FindFirstValue(TokenClaimTypes.EMAIL_VERIFIED),
                out var emailVerified) && emailVerified;
            if (string.IsNullOrWhiteSpace(providerUserId) || !verified)
            {
                throw InvalidToken();
            }

            // Step 3: Return a normalised profile only when a verified provider email is present.
            var verifiedEmail = EnsureEmail(email);
            var hostedDomain = principal.FindFirstValue("hd");
            var authoritative = verifiedEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
                                || !string.IsNullOrWhiteSpace(hostedDomain);
            return new ExternalIdentityProfileResponseDto
            {
                ProviderUserId = providerUserId,
                Email = verifiedEmail.NormalizeEmail(),
                FullName = principal.FindFirstValue(TokenClaimTypes.NAME)
                           ?? principal.FindFirstValue(ClaimTypes.Name),
                EmailVerified = true,
                CanAutoLinkByEmail = authoritative
            };
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, InfrastructureLogConstants.ExternalProviderLogs.TOKEN_VALIDATION_FAILED, options.Name);
            throw ProviderUnavailable();
        }
    }

    /// <summary>
    /// Gets a trusted Facebook identity after validating the User Access Token through Meta Graph.
    /// </summary>
    /// <param name="token">The Facebook User Access Token.</param>
    /// <param name="cancellationToken">The token used to cancel Meta Graph requests.</param>
    /// <returns>The trusted Facebook identity profile.</returns>
    private async Task<ExternalIdentityProfileResponseDto> GetFacebookIdentityAsync(
        string token,
        CancellationToken cancellationToken)
    {
        var options = GetProvider(ApplicationConstants.EXTERNAL_PROVIDER_FACEBOOK);
        try
        {
            // Step 1: Inspect token ownership and lifetime with app credentials kept outside the loggable request URI.
            var appAccessToken = $"{options.AppId}|{options.AppSecret}";
            using var debugResponse = await SendFacebookFormAsync(
                options,
                "debug_token",
                appAccessToken,
                new Dictionary<string, string>
                {
                    ["method"] = "GET",
                    ["input_token"] = token
                },
                cancellationToken);
            EnsureFacebookResponseStatus(debugResponse);

            using var debugJson = await JsonDocument.ParseAsync(
                await debugResponse.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            var data = debugJson.RootElement.GetProperty("data");
            var valid = data.TryGetProperty("is_valid", out var validElement) && validElement.GetBoolean();
            var appId = data.TryGetProperty("app_id", out var appIdElement) ? appIdElement.GetString() : null;
            var userId = data.TryGetProperty("user_id", out var userIdElement) ? userIdElement.GetString() : null;
            var expiresAt = data.TryGetProperty("expires_at", out var expiresElement) ? expiresElement.GetInt64() : 0;
            if (!valid
                || !string.Equals(appId, options.AppId, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(userId)
                || expiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                throw InvalidToken();
            }

            // Step 2: Fetch the profile with app-secret proof only after debug_token confirms ownership and lifetime.
            var proof = Convert.ToHexString(
                    HMACSHA256.HashData(
                        Encoding.UTF8.GetBytes(options.AppSecret),
                        Encoding.UTF8.GetBytes(token)))
                .ToLowerInvariant();
            using var profileResponse = await SendFacebookFormAsync(
                options,
                "me",
                token,
                new Dictionary<string, string>
                {
                    ["method"] = "GET",
                    ["fields"] = "id,name,email",
                    ["appsecret_proof"] = proof
                },
                cancellationToken);
            EnsureFacebookResponseStatus(profileResponse);

            using var profileJson = await JsonDocument.ParseAsync(
                await profileResponse.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            var root = profileJson.RootElement;
            var profileId = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            var email = root.TryGetProperty("email", out var emailElement) ? emailElement.GetString() : null;
            if (!string.Equals(profileId, userId, StringComparison.Ordinal))
            {
                throw InvalidToken();
            }

            // Step 3: Return a normalised profile only when /me confirms the same provider user and supplies an email address.
            var verifiedEmail = EnsureEmail(email);
            return new ExternalIdentityProfileResponseDto
            {
                ProviderUserId = userId,
                Email = verifiedEmail.NormalizeEmail(),
                FullName = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null,
                EmailVerified = true,
                CanAutoLinkByEmail = true
            };
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is HttpRequestException or JsonException or KeyNotFoundException or InvalidOperationException)
        {
            _logger.LogError(exception, InfrastructureLogConstants.ExternalProviderLogs.TOKEN_VALIDATION_FAILED, options.Name);
            throw ProviderUnavailable();
        }
    }

    /// <summary>
    /// Resolves validated startup options for one supported external provider.
    /// </summary>
    /// <param name="provider">The provider name.</param>
    /// <returns>The matching provider options.</returns>
    private ExternalProviderOptions GetProvider(string provider)
    {
        // Startup validation guarantees one matching configuration for every supported provider branch.
        return _options.Providers.First(x => x.Name == provider);
    }

    /// <summary>
    /// Creates the long-lived Google OIDC metadata manager reused across validation requests.
    /// </summary>
    /// <param name="options">The Google provider options.</param>
    /// <returns>The configured OIDC metadata manager.</returns>
    private static ConfigurationManager<OpenIdConnectConfiguration> CreateMetadataManager(ExternalProviderOptions options)
    {
        // Prefer an explicit metadata address and otherwise derive the standard discovery endpoint from authority.
        var metadataAddress = string.IsNullOrWhiteSpace(options.MetadataAddress)
            ? $"{options.Authority.TrimEnd('/')}/.well-known/openid-configuration"
            : options.MetadataAddress;
        return new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = metadataAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase) });
    }

    /// <summary>
    /// Sends a Meta Graph request while keeping credentials out of the request URI.
    /// </summary>
    /// <param name="options">The Facebook provider options.</param>
    /// <param name="path">The versioned Graph resource path.</param>
    /// <param name="bearerToken">The credential supplied through the authorisation header.</param>
    /// <param name="fields">The form fields sent to Meta Graph.</param>
    /// <param name="cancellationToken">The token used to cancel the HTTP request.</param>
    /// <returns>The provider HTTP response.</returns>
    private async Task<HttpResponseMessage> SendFacebookFormAsync(
        ExternalProviderOptions options,
        string path,
        string bearerToken,
        IReadOnlyDictionary<string, string> fields,
        CancellationToken cancellationToken)
    {
        // Keep credentials in authorisation/form content so standard HttpClient URI logging cannot expose them.
        var requestUri = $"{options.GraphApiBaseUrl.TrimEnd('/')}/{options.GraphApiVersion.Trim('/')}/{path}";
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content = new FormUrlEncodedContent(fields);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

        return await _httpClientFactory.CreateClient(nameof(ExternalIdentityProviderService))
            .SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Translates Meta Graph response status into stable credential or availability errors.
    /// </summary>
    /// <param name="response">The Meta Graph response.</param>
    private static void EnsureFacebookResponseStatus(HttpResponseMessage response)
    {
        // Rate limits and server failures represent provider availability, not invalid user credentials.
        if (response.StatusCode == HttpStatusCode.TooManyRequests
            || (int)response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            throw ProviderUnavailable();
        }

        // Remaining non-success responses represent a rejected provider credential.
        if (!response.IsSuccessStatusCode)
        {
            throw InvalidToken();
        }
    }

    /// <summary>
    /// Ensures the provider supplied the email required by Haven account flows.
    /// </summary>
    /// <param name="email">The provider-owned email claim or profile field.</param>
    private static string EnsureEmail(string email)
    {
        // Haven cannot link or prefill an account when the provider omits email.
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ApiException(
                ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_EMAIL_REQUIRED_MESSAGE,
                ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_EMAIL_REQUIRED,
                StatusCodes.Status400BadRequest);
        }

        return email;
    }

    /// <summary>
    /// Creates the stable unauthorised error for rejected external credentials.
    /// </summary>
    /// <returns>The invalid-provider-token API exception.</returns>
    private static ApiException InvalidToken()
    {
        // Use one public contract for every invalid or expired provider credential.
        return new ApiException(
            ApplicationErrorConstants.ExternalProviderErrors.INVALID_EXTERNAL_TOKEN_MESSAGE,
            ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_INVALID,
            StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Creates the stable service-unavailable error for provider or metadata outages.
    /// </summary>
    /// <returns>The provider-unavailable HTTP exception.</returns>
    private static HttpStatusCodeException ProviderUnavailable()
    {
        // Keep transient provider failures distinct from user credential rejection.
        return new HttpStatusCodeException(
            ApplicationErrorConstants.ExternalProviderErrors.EXTERNAL_PROVIDER_UNAVAILABLE_MESSAGE,
            ApplicationErrorConstants.ExternalProviderErrorCodes.AUTH_EXTERNAL_PROVIDER_UNAVAILABLE,
            StatusCodes.Status503ServiceUnavailable);
    }
}
