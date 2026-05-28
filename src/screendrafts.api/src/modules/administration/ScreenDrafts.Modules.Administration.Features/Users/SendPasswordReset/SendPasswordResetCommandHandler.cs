namespace ScreenDrafts.Modules.Administration.Features.Users.SendPasswordReset;

internal sealed class SendPasswordResetCommandHandler(
  IUsersApi usersApi,
  IAdministrationIdentityProviderService administrationIdentityProviderService
) : ICommandHandler<SendPasswordResetCommand>
{
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IAdministrationIdentityProviderService _administrationIdentityProviderService =
    administrationIdentityProviderService;

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

    return await _administrationIdentityProviderService.SendPasswordResetEmailAsync(
      user.IdentityId,
      cancellationToken
    );
  }
}
