using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed class IdentityProviderService(
  KeyCloakClient keyCloakClient,
  ILogger<IdentityProviderService> logger)
  : IIdentityProviderService
{
  private readonly KeyCloakClient _keyCloakClient = keyCloakClient;
  private readonly ILogger<IdentityProviderService> _logger = logger;

  private const string PasswordCredentialType = "password";

  // POST /admin/realms/{realm}/users
  public async Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
  {
    var userRepresentation = new UserRepresentation(
      user.Email,
      user.Email,
      user.FirstName,
      user.LastName,
      true,
      true,
      [new CredentialRepresentation(PasswordCredentialType, user.Password, false)]);

    try
    {
      string identityId = await _keyCloakClient.RegisterUserAsync(userRepresentation, cancellationToken);

      return identityId;
    }
    catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
    {
      IdentityMessages.UserRegistrationFailed(_logger, user.Email);

      return Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique);
    }
  }
}
