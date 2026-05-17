namespace Haven.Api.Controllers;

/// <summary>
/// Represents the base API controller providing common functionality for all API controllers.
/// </summary>
/// <example>
/// Derive controller classes from this base class to inherit shared behaviors.
/// </example>
[ApiController]
[Authorize]
[Route("v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
}
