namespace ScreenDrafts.Modules.Users.Features.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
  IUserRepository userRepository,
  IIdentityProviderService identityProviderService
) : ICommandHandler<ChangePasswordCommand>
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IIdentityProviderService _identityProviderService = identityProviderService;

  public async Task<Result> Handle(
    ChangePasswordCommand request,
    CancellationToken cancellationToken
  )
  {
    var user = await _userRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.PublicIdNotFound(request.PublicId));
    }

    return await _identityProviderService.ChangePasswordAsync(
      user.IdentityId,
      request.CurrentPassword,
      request.Password,
      user.Email.Value!,
      cancellationToken
    );
  }
}
