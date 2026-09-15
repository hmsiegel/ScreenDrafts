namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ValidateEmailBootstrapToken;

internal sealed class ValidateEmailBootstrapTokenQueryHandler(
  IEmailBootstrapTokenService tokenService,
  IEmailBootstrapClaimRepository claimRepository,
  IUserRepository userRepository
) : IQueryHandler<ValidateEmailBootstrapTokenQuery, EmailBootstrapTokenValidationResponse>
{
  private readonly IEmailBootstrapTokenService _tokenService = tokenService;
  private readonly IEmailBootstrapClaimRepository _claimRepository = claimRepository;
  private readonly IUserRepository _userRepository = userRepository;

  public async Task<Result<EmailBootstrapTokenValidationResponse>> Handle(
    ValidateEmailBootstrapTokenQuery request,
    CancellationToken cancellationToken
  )
  {
    var payloadResult = _tokenService.ValidateToken(request.Token);

    if (payloadResult.IsFailure)
    {
      return Result.Failure<EmailBootstrapTokenValidationResponse>(payloadResult.Errors[0]);
    }

    var claim = await _claimRepository.GetByUserIdAsync(
      payloadResult.Value.UserId,
      cancellationToken
    );

    if (claim is null)
    {
      return Result.Failure<EmailBootstrapTokenValidationResponse>(
        EmailBootstrapClaimErrors.NotFound(payloadResult.Value.UserId.Value)
      );
    }

    if (claim.IsClaimed)
    {
      return Result.Failure<EmailBootstrapTokenValidationResponse>(
        EmailBootstrapClaimErrors.AlreadyClaimed
      );
    }

    if (claim.ExpiresAt < DateTimeOffset.UtcNow)
    {
      return Result.Failure<EmailBootstrapTokenValidationResponse>(
        EmailBootstrapClaimErrors.Expired
      );
    }

    var user = await _userRepository.GetAsync(payloadResult.Value.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure<EmailBootstrapTokenValidationResponse>(
        UserErrors.NotFound(payloadResult.Value.UserId.Value)
      );
    }

    return Result.Success(
      new EmailBootstrapTokenValidationResponse { IsValid = true, UserPublicId = user.PublicId }
    );
  }
}
