namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class UserRegisteredIntegrationEventConsumer(IDbConnectionFactory dbConnectionFactory)
  : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public override async Task Handle(
    UserRegisteredIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      INSERT INTO communications.user_emails (user_id, email_address, full_name)
      VALUES (@UserId, @Email, @FullName)
      ON CONFLICT (user_id) DO UPDATE
      SET email_address = EXCLUDED.email_address,
          full_name = EXCLUDED.full_name;
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        commandText: sql,
        parameters: new
        {
          integrationEvent.UserId,
          integrationEvent.Email,
          FullName = $"{integrationEvent.FirstName} {integrationEvent.LastName}"
        },
        cancellationToken: cancellationToken));

  }
}
