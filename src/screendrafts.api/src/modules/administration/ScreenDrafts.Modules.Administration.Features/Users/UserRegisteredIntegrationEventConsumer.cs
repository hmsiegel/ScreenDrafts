namespace ScreenDrafts.Modules.Administration.Features.Users;

internal sealed partial class UserRegisteredIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  ILogger<UserRegisteredIntegrationEventConsumer> logger,
  IDateTimeProvider dateTimeProvider)
  : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly ILogger<UserRegisteredIntegrationEventConsumer> _logger = logger;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    UserRegisteredIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    try
    {
      const string insertRoleSql =
    """
      INSERT INTO administration.user_roles (user_id, role_name)
      VALUES (@UserId, @RoleName)
      ON CONFLICT (user_id, role_name) DO NOTHING
      """;

      await connection.ExecuteAsync(
        new CommandDefinition(
          insertRoleSql,
          new
          {
            integrationEvent.UserId,
            RoleName = "Guest"
          },
          cancellationToken: cancellationToken));

      const string permSql =
        $"""
      SELECT permission_code AS {nameof(PermissionRow.PermissionCode)}
      FROM administration.role_permissions
      WHERE role_name = 'Guest'
      """;

      var permissionCodes = (await connection.QueryAsync<PermissionRow>(
        new CommandDefinition(
          permSql,
          cancellationToken: cancellationToken)))
        .Select(p => p.PermissionCode)
        .ToList();

      await _eventBus.PublishAsync(
        new UserRoleAddedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: _dateTimeProvider.UtcNow,
          userId: integrationEvent.UserId,
          roleName: "Guest",
          permissionCodesToAdd: permissionCodes),
        cancellationToken);

    }
    catch (Exception ex)
    {
      LogFailedToAssignGuestRole(_logger, integrationEvent.UserId, ex);
      throw new ScreenDraftsException(nameof(UserRegisteredIntegrationEventConsumer), ex);
    }  }

  private sealed record PermissionRow(string PermissionCode);

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Failed to assign guest role to newly registered user {UserId}.")]
  private static partial void LogFailedToAssignGuestRole(ILogger logger, Guid userId, Exception ex);
}
