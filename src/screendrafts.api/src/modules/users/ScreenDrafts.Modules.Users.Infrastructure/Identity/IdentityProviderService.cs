namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed partial class IdentityProviderService(
  KeyCloakClient keyCloakClient,
  ILogger<IdentityProviderService> logger,
  IOptions<KeyCloakOptions> keyCloakOptions
) : IIdentityProviderService
{
  private readonly KeyCloakClient _keyCloakClient = keyCloakClient;
  private readonly ILogger<IdentityProviderService> _logger = logger;
  private readonly KeyCloakOptions _keyCloakOptions = keyCloakOptions.Value;

  private const string PasswordCredentialType = "password";

  // POST /admin/realms/{realm}/users
  public async Task<Result<string>> RegisterUserAsync(
    UserModel user,
    CancellationToken cancellationToken = default
  )
  {
    var attributes = user.PublicId is not null
      ? new Dictionary<string, List<string>> { ["public_id"] = [user.PublicId] }
      : null;

    var userRepresentation = new UserRepresentation(
      user.Email,
      user.Email,
      user.FirstName,
      user.LastName,
      true,
      true,
      [new CredentialRepresentation(PasswordCredentialType, user.Password, false)],
      attributes
    );

    try
    {
      string identityId = await _keyCloakClient.RegisterUserAsync(
        userRepresentation,
        cancellationToken
      );

      return identityId;
    }
    catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
    {
      IdentityMessages.UserRegistrationFailed(_logger, user.Email);

      return Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique);
    }
  }

  public async Task<Result> ChangePasswordAsync(
    string identityId,
    string currentPassword,
    string newPassword,
    string userEmail,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      // Step 1: verify the current password is correct before touching Keycloak admin API.
      await KeyCloakClient.VerifyPasswordAsync(
        userEmail,
        currentPassword,
        _keyCloakOptions,
        cancellationToken
      );
    }
    catch (InvalidOperationException)
    {
      return Result.Failure(IdentityProviderErrors.InvalidCurrentPassword);
    }

    try
    {
      // Step 2: reset to the new password via the admin API.
      await _keyCloakClient.ResetPasswordAsync(identityId, newPassword, cancellationToken);
    }
    catch (HttpRequestException exception)
    {
      PasswordChangeFailed(_logger, identityId, exception);
      return Result.Failure(IdentityProviderErrors.PasswordChangeFailed);
    }

    return Result.Success();
  }

  public async Task<Result> UpdateEmailAsync(
    string identityId,
    string newEmail,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      await _keyCloakClient.UpdateEmailAsync(identityId, newEmail, cancellationToken);
      return Result.Success();
    }
    catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
    {
      EmailUpdateConflict(_logger, identityId, newEmail);

      // Reusing EmailIsNotUnique rather than inventing a new IdentityProviderErrors
      // member sight-unseen — I don't have that file, only the two members already
      // used elsewhere in this class. Add a dedicated error there if you'd rather
      // distinguish "email in use at registration" from "email in use at claim".
      return Result.Failure(IdentityProviderErrors.EmailIsNotUnique);
    }
    catch (HttpRequestException exception)
    {
      EmailUpdateFailed(_logger, identityId, exception);
      return Result.Failure(IdentityProviderErrors.PasswordChangeFailed);
    }
  }

  public async Task<Result> TriggerPasswordResetAsync(
    string identityId,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      await _keyCloakClient.SendPasswordResetEmailAsync(identityId, cancellationToken);
      return Result.Success();
    }
    catch (HttpRequestException exception)
    {
      PasswordResetTriggerFailed(_logger, identityId, exception);
      return Result.Failure(IdentityProviderErrors.PasswordChangeFailed);
    }
  }

  [LoggerMessage(
    Level = LogLevel.Error,
    Message = "Failed to change password for identity {IdentityId}."
  )]
  private static partial void PasswordChangeFailed(
    ILogger<IdentityProviderService> logger,
    string identityId,
    Exception ex
  );

  [LoggerMessage(
    Level = LogLevel.Warning,
    Message = "Email update for identity {IdentityId} to {NewEmail} conflicted — already in use."
  )]
  private static partial void EmailUpdateConflict(
    ILogger<IdentityProviderService> logger,
    string identityId,
    string newEmail
  );

  [LoggerMessage(
    Level = LogLevel.Error,
    Message = "Failed to update email for identity {IdentityId}."
  )]
  private static partial void EmailUpdateFailed(
    ILogger<IdentityProviderService> logger,
    string identityId,
    Exception ex
  );

  [LoggerMessage(
    Level = LogLevel.Error,
    Message = "Failed to trigger password reset email for identity {IdentityId}."
  )]
  private static partial void PasswordResetTriggerFailed(
    ILogger<IdentityProviderService> logger,
    string identityId,
    Exception ex
  );
}
