namespace Authentication.Application.Commands.Kyc;

/// <summary>
/// Handles current-user identity-document KYC submissions.
/// </summary>
public class SubmitKycCommandHandler : ICommandHandler<SubmitKycCommand, ResponseDto<KycSubmissionResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ILogger<SubmitKycCommandHandler> _logger;

    /// <summary>
    /// Creates the KYC submission handler with current-user services and flow logging.
    /// </summary>
    /// <param name="userService">The service that persists KYC data and private document metadata.</param>
    /// <param name="authService">The service that reads the current authenticated principal.</param>
    /// <param name="logger">The logger used for KYC submission completion tracking.</param>
    public SubmitKycCommandHandler(
        IUserService userService,
        IAuthService authService,
        ILogger<SubmitKycCommandHandler> logger)
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the current user's KYC submission.
    /// </summary>
    /// <param name="request">The KYC form and file payload.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The standardized response containing KYC submission status.</returns>
    public async ValueTask<ResponseDto<KycSubmissionResponseDto>> Handle(
        SubmitKycCommand request,
        CancellationToken cancellationToken)
    {
        // KYC submission belongs to the authenticated account; no document identity is accepted from route/query state.
        var currentUserPublicId = _authService.UserId()
                                  ?? throw new HttpStatusCodeException(
                                      UNAUTHORIZED_REQUEST_MESSAGE,
                                      UNAUTHORIZED,
                                      StatusCodes.Status401Unauthorized);

        // UserService stores the scanned identity snapshot for manual review; profile sync waits for admin approval.
        var result = await _userService.SubmitKycAsync(request.Adapt<SubmitKycRequestDto>(), cancellationToken);
        _logger.LogInformation(KYC_SUBMITTED, currentUserPublicId);

        return new ResponseDto<KycSubmissionResponseDto>(result);
    }
}
