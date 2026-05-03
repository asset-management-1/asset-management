namespace Authentication.Application.Commands.ExternalLink;

public class LinkExternalProviderCommandHandler : ICommandHandler<LinkExternalProviderCommand, ResponseDto<string>>
{
    private readonly IAuthenticationService _authenticationService;

    public LinkExternalProviderCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<ResponseDto<string>> Handle(LinkExternalProviderCommand request, CancellationToken cancellationToken)
    {
        return _authenticationService.LinkExternalProviderAsync(request, cancellationToken);
    }
}
