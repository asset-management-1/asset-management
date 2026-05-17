namespace Authentication.Application.Commands.RefreshToken;

/// <summary>
/// Validates refresh-token exchange requests.
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// Creates validation rules requiring a refresh token value.
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .Required();
    }
}
