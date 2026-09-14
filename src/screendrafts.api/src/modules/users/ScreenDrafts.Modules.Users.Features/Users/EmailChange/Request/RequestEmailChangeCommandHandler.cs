namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Request;

internal sealed class RequestEmailChangeCommandHandler(
  IUserRepository userRepository,
  IEmailChangeTokenRepository tokenRepository,
  IOptions<EmailChangeOptions> options,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<RequestEmailChangeCommand>
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IEmailChangeTokenRepository _tokenRepository = tokenRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly EmailChangeOptions _options = options.Value;

  public async Task<Result> Handle(
    RequestEmailChangeCommand request,
    CancellationToken cancellationToken
  )
  {
    var user = await _userRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.PublicIdNotFound(request.PublicId));
    }

    var emailResult = Email.Create(request.NewEmail);

    if (emailResult.IsFailure)
    {
      return Result.Failure(emailResult.Errors[0]);
    }

    if (user.Email == emailResult.Value)
    {
      return Result.Failure(EmailChangeErrors.SameAsCurrentEmail);
    }

    if (!await _userRepository.IsEmailUniqueAsync(emailResult.Value, cancellationToken))
    {
      return Result.Failure(UserErrors.EmailInUse);
    }

    // Only one active token per user — superseding a request they never
    // confirmed rather than letting two valid links exist at once.
    var existingToken = await _tokenRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);

    if (existingToken is not null)
    {
      existingToken.Invalidate(_dateTimeProvider.UtcTimeZoneNow);
      _tokenRepository.Update(existingToken);
    }

    var rawToken = GenerateRawToken();
    var tokenHash = HashToken(rawToken);

    var now = _dateTimeProvider.UtcTimeZoneNow;
    var expiresAt = now.AddHours(_options.TokenExpiryHours);

    _tokenRepository.Add(
      EmailChangeToken.Issue(user.Id, emailResult.Value.Value!, tokenHash, now, expiresAt)
    );

    var confirmationLink = $"{_options.ConfirmationBaseUrl}?token={Uri.EscapeDataString(rawToken)}";

    user.RequestEmailChange(emailResult.Value.Value!, confirmationLink);
    _userRepository.Update(user);

    return Result.Success();
  }

  private static string GenerateRawToken() =>
    Convert
      .ToBase64String(RandomNumberGenerator.GetBytes(32))
      .TrimEnd('=')
      .Replace('+', '-')
      .Replace('/', '_');

  private static string HashToken(string rawToken) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
