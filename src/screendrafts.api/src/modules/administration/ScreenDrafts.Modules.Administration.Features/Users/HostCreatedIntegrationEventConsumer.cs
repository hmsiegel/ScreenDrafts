namespace ScreenDrafts.Modules.Administration.Features.Users;

internal sealed partial class HostCreatedIntegrationEventConsumer(
  ISender sender,
  IUsersApi usersApi,
  ILogger<HostCreatedIntegrationEventConsumer> logger
) : IntegrationEventHandler<HostCreatedIntegrationEvent>
{
  private readonly ISender _sender = sender;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly ILogger<HostCreatedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    HostCreatedIntegrationEvent integrationEvent,
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
      RoleName = AdministrationAuth.Roles.Host,
    };

    var result = await _sender.Send(command, cancellationToken);

    if (result.IsFailure)
    {
      LogFailedToAddRole(_logger, integrationEvent.UserPublicId, AdministrationAuth.Roles.Host);
    }
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Error,
    Message = "User with public ID {PublicId} not found when handling HostCreatedIntegrationEvent."
  )]
  private static partial void LogUserNotFound(ILogger logger, string publicId);

  [LoggerMessage(
    EventId = 1002,
    Level = LogLevel.Error,
    Message = "Failed to add role {RoleName} to user with public ID {PublicId} when handling HostCreatedIntegrationEvent."
  )]
  private static partial void LogFailedToAddRole(ILogger logger, string publicId, string roleName);
}
