namespace ScreenDrafts.Modules.Administration.Features.Users.SendPasswordReset;

internal sealed class SendPasswordResetCommandHandler(
  IUsersApi usersApi,
  IAdministrationIdentityProviderService administrationIdentityProviderService,
  IConfiguration configuration
) : ICommandHandler<SendPasswordResetCommand>
{
  private const string AllowPlaceholderRecipients =
    "Communications:Smtp:AllowPlaceholderRecipients";

  private readonly IUsersApi _usersApi = usersApi;
  private readonly IAdministrationIdentityProviderService _administrationIdentityProviderService =
    administrationIdentityProviderService;
  private readonly bool _allowPlaceholderRecipients =
    bool.TryParse(configuration[AllowPlaceholderRecipients], out var result) && result;

  public async Task<Result> Handle(
    SendPasswordResetCommand request,
    CancellationToken cancellationToken
  )
  {
    var user = await _usersApi.GetUserByPublicId(request.PublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(AdministrationErrors.UserNotFound(request.PublicId));
    }

    if (!_allowPlaceholderRecipients && PlaceholderEmail.IsPlaceholder(user.Email))
    {
      return Result.Failure(AdministrationErrors.PasswordResetEmailUndeliverable);
    }

    return await _administrationIdentityProviderService.SendPasswordResetEmailAsync(
      user.IdentityId,
      cancellationToken
    );
  }
}
