namespace ScreenDrafts.Modules.Users.Features.Abstractions.Identity;

public interface IIdentityProviderService
{
  Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
}
