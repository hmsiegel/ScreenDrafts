namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class UserEmailChangedIntegrationEventConsumer(
  IDbConnectionFactory dbConnectionFactory
) : IntegrationEventHandler<UserEmailChangedIntegrationEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public override async Task Handle(
    UserEmailChangedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // A row should always already exist here (created at registration) — plain
    // UPDATE rather than the UPSERT used at registration time.
    const string sql = """
      UPDATE communications.user_emails
      SET email_address = @NewEmail
      WHERE user_id = @UserId;
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        commandText: sql,
        parameters: new { integrationEvent.UserId, integrationEvent.NewEmail },
        cancellationToken: cancellationToken
      )
    );
  }
}
