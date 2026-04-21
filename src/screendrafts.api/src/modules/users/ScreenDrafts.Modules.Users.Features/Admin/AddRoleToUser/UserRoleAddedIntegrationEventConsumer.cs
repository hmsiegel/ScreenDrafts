namespace ScreenDrafts.Modules.Users.Features.Admin.AddRoleToUser;

internal sealed partial class UserRoleAddedIntegrationEventConsumer(
  IDbConnectionFactory dbConnectionFactory,
  ILogger<UserRoleAddedIntegrationEventConsumer> logger)
  : IntegrationEventHandler<UserRoleAddedIntegrationEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ILogger<UserRoleAddedIntegrationEventConsumer> _logger = logger;

  public override async Task Handle(
    UserRoleAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    if (integrationEvent.PermissionCodesToAdd.Count == 0)
    {
      return;
    }

    try
    {
      await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

      const string sql = """
        INSERT INTO users.user_permissions (user_id, permission_code)
        VALUES (@UserId, @PermissionCodeToAdd)
        ON CONFLICT (user_id, permission_code) DO NOTHING
        """;

      var rows = integrationEvent.PermissionCodesToAdd
        .Select(code => new
        {
          integrationEvent.UserId,
          PermissionCodeToAdd = code
        });

      await connection.ExecuteAsync(sql, rows);
    }
    catch (Exception ex)
    {
      LogFailedToUpdateUserPermissions(_logger, integrationEvent.UserId, integrationEvent.RoleName);
      throw new ScreenDraftsException(nameof(UserRoleAddedIntegrationEventConsumer), ex);
    }
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Error,
    Message = "Failed to update user_permissions for user {UserId} after role {RoleName} was added.")]
  private static partial void LogFailedToUpdateUserPermissions(ILogger logger, Guid userId, string roleName);
}
