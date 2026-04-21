namespace ScreenDrafts.Modules.Users.Features.Admin.RemovePermissionFromRole;

internal sealed partial class PermissionRemovedFromRoleIntegrationEventConsumer(
  IDbConnectionFactory dbConnectionFactory,
  ILogger<PermissionRemovedFromRoleIntegrationEventConsumer> logger)
  : IntegrationEventHandler<PermissionRemovedFromRoleIntegrationEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ILogger<PermissionRemovedFromRoleIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    PermissionRemovedFromRoleIntegrationEvent integrationEvent,
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
      DELETE FROM users.user_permissions
      WHERE user_id = @UserId
        AND permission_code = @PermissionCode
      """;

      var rows = integrationEvent.AffectedUserIds
        .Select(userId => new
        {
          UserId = userId,
          integrationEvent.PermissionCode
        })
        .ToList();

      await connection.ExecuteAsync(sql, rows);
    }
    catch (Exception ex)
    {
      LogFailedToUpdateUserPermissions(_logger, integrationEvent.RoleName, integrationEvent.PermissionCode);
      throw new ScreenDraftsException(nameof(PermissionRemovedFromRoleIntegrationEvent), ex);
    }

  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Failed to update user_permissions for users with role {RoleName} after permission {PermissionCode} was removed.")]
  private static partial void LogFailedToUpdateUserPermissions(ILogger logger, string roleName, string permissionCode);
}
