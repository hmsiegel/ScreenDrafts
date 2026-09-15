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

  /// <summary>Updates the email on the Keycloak side and marks it verified.</summary>
  Task<Result> UpdateEmailAsync(
    string identityId,
    string newEmail,
    CancellationToken cancellationToken = default
  );

  /// <summary>Fires Keycloak's native UPDATE_PASSWORD action email.</summary>
  Task<Result> TriggerPasswordResetAsync(
    string identityId,
    CancellationToken cancellationToken = default
  );
}
