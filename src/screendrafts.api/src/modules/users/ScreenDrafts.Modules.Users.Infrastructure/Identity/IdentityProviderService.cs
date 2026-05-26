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

  [LoggerMessage(
    Level = LogLevel.Error,
    Message = "Failed to change password for identity {IdentityId}."
  )]
  public static partial void PasswordChangeFailed(
    ILogger<IdentityProviderService> logger,
    string identityId,
    Exception ex
  );
}
