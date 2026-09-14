namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Confirm;

internal sealed class ConfirmEmailChangeCommandHandler(
  IEmailChangeTokenRepository tokenRepository,
  IUserRepository userRepository,
  IIdentityProviderService identityProviderService,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<ConfirmEmailChangeCommand>
{
  private readonly IEmailChangeTokenRepository _tokenRepository = tokenRepository;
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IIdentityProviderService _identityProviderService = identityProviderService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    ConfirmEmailChangeCommand request,
    CancellationToken cancellationToken
  )
  {
    var tokenHash = HashToken(request.Token);

    var token = await _tokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

    if (token is null)
    {
      return Result.Failure(EmailChangeErrors.InvalidToken);
    }

    if (token.IsUsed)
    {
      return Result.Failure(EmailChangeErrors.AlreadyUsed);
    }

    if (token.ExpiresAt < DateTimeOffset.UtcNow)
    {
      return Result.Failure(EmailChangeErrors.Expired);
    }

    var user = await _userRepository.GetAsync(token.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.NotFound(token.UserId.Value));
    }

    var emailResult = Email.Create(token.NewEmail);

    if (emailResult.IsFailure)
    {
      return Result.Failure(emailResult.Errors[0]);
    }

    // Defense in depth — re-check uniqueness in case someone else claimed this
    // address in the window between request and confirm.
    if (!await _userRepository.IsEmailUniqueAsync(emailResult.Value, cancellationToken))
    {
      return Result.Failure(UserErrors.EmailInUse);
    }

    var updateEmailResult = await _identityProviderService.UpdateEmailAsync(
      identityId: user.IdentityId,
      newEmail: token.NewEmail,
      cancellationToken: cancellationToken
    );

    if (updateEmailResult.IsFailure)
    {
      return updateEmailResult;
    }

    var markUsedResult = token.MarkUsed(_dateTimeProvider.UtcTimeZoneNow);

    if (markUsedResult.IsFailure)
    {
      return markUsedResult;
    }

    _tokenRepository.Update(token);

    user.ChangeEmail(emailResult.Value);
    _userRepository.Update(user);

    return Result.Success();
  }

  private static string HashToken(string rawToken) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
