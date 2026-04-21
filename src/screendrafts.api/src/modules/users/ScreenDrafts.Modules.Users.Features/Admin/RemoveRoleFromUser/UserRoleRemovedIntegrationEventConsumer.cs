namespace ScreenDrafts.Modules.Users.Features.Admin.RemoveRoleFromUser;

internal sealed partial class UserRoleRemovedIntegrationEventConsumer(
  IDbConnectionFactory dbConnectionFactory,
  ILogger<UserRoleRemovedIntegrationEvent> logger)
  : IntegrationEventHandler<UserRoleRemovedIntegrationEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ILogger<UserRoleRemovedIntegrationEvent> _logger = logger;

  public override async Task Handle(
    UserRoleRemovedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    if (integrationEvent.PermissionCodesToRemove.Count == 0)
    {
      return;
    }

    try
    {
      await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

      const string sql =
        """
        DELETE FROM users.user_permissions
        WHERE user_id = @UserId
          AND permission_code = @PermissionCode
        """;

      var rows = integrationEvent.PermissionCodesToRemove
        .Select(code => new
        {
          integrationEvent.UserId,
          PermissionCode = code
        })
        .ToList();

      await connection.ExecuteAsync(sql, rows);
    }
    catch (Exception ex)
    {
      LogFailedToUpdateUserPermissions(_logger, integrationEvent.UserId, integrationEvent.RoleName);
      throw new ScreenDraftsException(nameof(UserRoleRemovedIntegrationEvent), ex);
    }
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Failed to update user_permissions for user {UserId} after role {RoleName} was removed.")]
  private static partial void LogFailedToUpdateUserPermissions(ILogger logger, Guid userId, string roleName);
}
