namespace ScreenDrafts.Modules.Administration.Infrastructure.Identity;

internal sealed partial class AdministrationIdentityProviderService(
  KeyCloakClient keyCloakClient,
  ILogger<AdministrationIdentityProviderService> logger
) : IAdministrationIdentityProviderService
{
  private readonly KeyCloakClient _keyCloakClient = keyCloakClient;
  private readonly ILogger<AdministrationIdentityProviderService> _logger = logger;

  public async Task<Result> SendPasswordResetEmailAsync(
    string identityId,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      await _keyCloakClient.SendPasswordResetEmailAsync(identityId, cancellationToken);
      return Result.Success();
    }
    catch (HttpRequestException ex)
    {
      LogFailedToSendPasswordResetEmail(_logger, identityId, ex.Message);
      return Result.Failure(AdministrationErrors.PasswordResetEmailFailed);
    }
  }

  [LoggerMessage(
    Level = LogLevel.Error,
    Message = "Failed to send password reset email for identity {IdentityId}. Exception: {Exception}"
  )]
  private static partial void LogFailedToSendPasswordResetEmail(
    ILogger<AdministrationIdentityProviderService> logger,
    string identityId,
    string exception
  );
}
