namespace Authentication.Application.Queries.Logins;

public class GetLoginQuery : IQuery<ResponseDto<LoginResponse>>
{
    /// <summary>
    /// Gets or sets the username associated with the factory requesting the token.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the password used to authenticate the factory request.
    /// </summary>
    public string Password { get; set; }
}
