namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ClaimEmailBootstrap;

internal sealed class ClaimEmailBootstrapCommandHandler(
  IEmailBootstrapTokenService tokenService,
  IEmailBootstrapClaimRepository claimRepository,
  IUserRepository userRepository,
  IIdentityProviderService identityProviderService
) : ICommandHandler<ClaimEmailBootstrapCommand>
{
  private readonly IEmailBootstrapTokenService _tokenService = tokenService;
  private readonly IEmailBootstrapClaimRepository _claimRepository = claimRepository;
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IIdentityProviderService _identityProviderService = identityProviderService;

  public async Task<Result> Handle(
    ClaimEmailBootstrapCommand request,
    CancellationToken cancellationToken
  )
  {
    var payloadResult = _tokenService.ValidateToken(request.Token);

    if (payloadResult.IsFailure)
    {
      return Result.Failure(payloadResult.Errors[0]);
    }

    var claim = await _claimRepository.GetByUserIdAsync(
      payloadResult.Value.UserId,
      cancellationToken
    );

    if (claim is null)
    {
      return Result.Failure(EmailBootstrapClaimErrors.NotFound(payloadResult.Value.UserId.Value));
    }

    if (claim.IsClaimed)
    {
      return Result.Failure(EmailBootstrapClaimErrors.AlreadyClaimed);
    }

    if (claim.ExpiresAt < DateTimeOffset.UtcNow)
    {
      return Result.Failure(EmailBootstrapClaimErrors.Expired);
    }

    var emailResult = Email.Create(request.NewEmail);

    if (emailResult.IsFailure)
    {
      return Result.Failure(emailResult.Errors[0]);
    }

    if (!await _userRepository.IsEmailUniqueAsync(emailResult.Value, cancellationToken))
    {
      return Result.Failure(UserErrors.EmailInUse);
    }

    var user = await _userRepository.GetAsync(payloadResult.Value.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.NotFound(payloadResult.Value.UserId.Value));
    }

    var updateEmailResult = await _identityProviderService.UpdateEmailAsync(
      user.IdentityId,
      emailResult.Value.Value!,
      cancellationToken
    );

    if (updateEmailResult.IsFailure)
    {
      return updateEmailResult;
    }

    var resetResult = await _identityProviderService.TriggerPasswordResetAsync(
      user.IdentityId,
      cancellationToken
    );

    if (resetResult.IsFailure)
    {
      // Logged inside TriggerPasswordResetAsync already — nothing further to do
      // here beyond letting the claim proceed.
    }

    var claimResult = claim.Claim(emailResult.Value.Value!, DateTimeOffset.UtcNow);

    if (claimResult.IsFailure)
    {
      return claimResult;
    }

    _claimRepository.Update(claim);

    user.ChangeEmail(emailResult.Value);
    _userRepository.Update(user);

    return Result.Success();
  }
}
