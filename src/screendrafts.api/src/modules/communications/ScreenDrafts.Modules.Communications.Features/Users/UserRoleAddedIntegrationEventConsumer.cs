namespace ScreenDrafts.Modules.Communications.Features.Users;

internal sealed class UserRoleAddedIntegrationEventConsumer(IDbConnectionFactory connectionFactory)
  : IntegrationEventHandler<UserRoleAddedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private const string PatreonRoleName = "Patreon";
  
  public override async Task Handle(UserRoleAddedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    if (!string.Equals(integrationEvent.RoleName, PatreonRoleName, StringComparison.OrdinalIgnoreCase))
    {
      return;
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = 
      """
      UPDATE communications.user_emails
      SET IsPatreon = TRUE
      WHERE user_id = @UserId;
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        commandText: sql,
        parameters: new { integrationEvent.UserId },
        cancellationToken: cancellationToken));

  }
}
