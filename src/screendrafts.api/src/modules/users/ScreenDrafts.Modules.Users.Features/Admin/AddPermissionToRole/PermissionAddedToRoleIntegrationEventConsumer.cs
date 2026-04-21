namespace ScreenDrafts.Modules.Users.Features.Admin.AddPermissionToRole;

internal sealed partial class PermissionAddedToRoleIntegrationEventConsumer(
  IDbConnectionFactory dbConnectionFactory,
  ILogger<PermissionAddedToRoleIntegrationEventConsumer> logger)
  : IntegrationEventHandler<PermissionAddedToRoleIntegrationEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ILogger<PermissionAddedToRoleIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    PermissionAddedToRoleIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    if (integrationEvent.AffectedUserIds.Count == 0)
    {
      return;
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    try
    {
      const string sql =
        """
      INSERT INTO users.user_permissions (user_id, permission_code)
      VALUES (@UserId, @PermissionCode)
      ON CONFLICT (user_id, permission_code) DO NOTHING
      """;

      var rows = integrationEvent.AffectedUserIds
        .Select(userId => new
        {
          UserId = userId,
          integrationEvent.PermissionCode
        });

      await connection.ExecuteAsync(sql, rows);
    }
    catch (Exception ex)
    {
      LogFailedToUpdateUserPermissions(_logger, integrationEvent.RoleName, integrationEvent.PermissionCode);
      throw new ScreenDraftsException(nameof(PermissionAddedToRoleIntegrationEvent), ex);
    }

  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Failed to update user_permissions for users with role {RoleName} after permission {PermissionCode} was added.")]
  private static partial void LogFailedToUpdateUserPermissions(ILogger logger, string roleName, string permissionCode);
}
