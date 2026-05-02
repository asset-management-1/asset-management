namespace Authentication.Application.Queries.Logins;

public class GetLoginQueryValidator : AbstractValidator<GetLoginQuery>
{
    public GetLoginQueryValidator()
    {
        RuleFor(x => x.UserName)
            .Required();

        RuleFor(x => x.Password)
            .Required();
    }
}
