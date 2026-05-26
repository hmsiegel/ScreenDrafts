namespace ScreenDrafts.Modules.Users.Domain.Abstractions.Identity;

public interface IIdentityProviderService
{
  Task<Result<string>> RegisterUserAsync(
    UserModel user,
    CancellationToken cancellationToken = default
  );
  Task<Result> ChangePasswordAsync(
    string identityId,
    string currentPassword,
    string newPassword,
    string userEmail,
    CancellationToken cancellationToken = default
  );
}
