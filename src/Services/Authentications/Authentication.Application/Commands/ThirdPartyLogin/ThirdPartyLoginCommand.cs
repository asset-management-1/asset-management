namespace Authentication.Application.Commands.ThirdPartyLogin;

public class ThirdPartyLoginCommand : ICommand<ResponseDto<LoginResponse>>
{
    /// <summary>
    /// Provider name (e.g., Google, Facebook, Apple).
    /// </summary>
    public string LoginProvider { get; set; }

    /// <summary>
    /// Provider unique key identifying the external account.
    /// </summary>
    public string ProviderKey { get; set; }

    /// <summary>
    /// Optional email returned by provider.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Optional full name returned by provider.
    /// </summary>
    public string FullName { get; set; }
}
