namespace ScreenDrafts.Modules.Communications.Features.Users;

internal sealed class UserRoleRemovedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory
) : IntegrationEventHandler<UserRoleRemovedIntegrationEvent>
{
  // Mirrors the role name in administration.roles. Communications can't reference
  // Administration's constants, so keep this in sync by hand.
  private const string PatreonRoleName = "Patreon";

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public override async Task Handle(
    UserRoleRemovedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    if (!string.Equals(integrationEvent.RoleName, PatreonRoleName, StringComparison.Ordinal))
    {
      return;
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      UPDATE communications.user_emails
      SET is_patreon = false
      WHERE user_id = @UserId
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        commandText: sql,
        parameters: new { integrationEvent.UserId },
        cancellationToken: cancellationToken
      )
    );
  }
}
