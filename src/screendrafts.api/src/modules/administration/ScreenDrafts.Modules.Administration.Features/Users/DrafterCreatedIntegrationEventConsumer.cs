namespace ScreenDrafts.Modules.Administration.Features.Users;

internal sealed partial class DrafterCreatedIntegrationEventConsumer(
  ISender sender,
  IUsersApi usersApi,
  ILogger<DrafterCreatedIntegrationEventConsumer> logger
) : IntegrationEventHandler<DrafterCreatedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly ILogger<DrafterCreatedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    DrafterCreatedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var user = await _usersApi.GetUserByPublicId(integrationEvent.UserPublicId, cancellationToken);

    if (user is null)
    {
      LogUserNotFound(_logger, integrationEvent.UserPublicId);
      return;
    }

    var command = new AddRoleToUserCommand
    {
      UserPublicId = integrationEvent.UserPublicId,
      RoleName = AdministrationAuth.Roles.Drafter,
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      LogFailedToAddRole(_logger, integrationEvent.UserPublicId, AdministrationAuth.Roles.Drafter);
    }
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Error,
    Message = "User with public ID {PublicId} not found when handling DrafterCreatedIntegrationEvent."
  )]
  private static partial void LogUserNotFound(ILogger logger, string publicId);

  [LoggerMessage(
    EventId = 1002,
    Level = LogLevel.Error,
    Message = "Failed to add role {RoleName} to user with public ID {PublicId} when handling DrafterCreatedIntegrationEvent."
  )]
  private static partial void LogFailedToAddRole(ILogger logger, string publicId, string roleName);
}
