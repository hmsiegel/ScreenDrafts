namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class DraftCreatedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService)
  : IntegrationEventHandler<DraftCreatedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;

  public override async Task Handle(DraftCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      SELECT
        email_address AS EmailAddress,
        full_name AS FullName
      FROM communications.user_emails
      WHERE @IsPatreon = false OR is_patreon = true
      """;

    var recipients = await connection.QueryAsync<RecipientRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { integrationEvent.IsPatreon },
        cancellationToken: cancellationToken));

    foreach (var recipient in recipients)
    {
      var html = EmailTemplates.DraftCreated(recipient.FullName, integrationEvent.DraftTitle, integrationEvent.IsPatreon);

      await _emailService.SendAsync(
        new EmailMessage
        {
          ToAddress = recipient.EmailAddress,
          ToName = recipient.FullName,
          Subject = integrationEvent.IsPatreon
            ? $"[Patreon] New Draft incoming: {integrationEvent.DraftTitle}"
            : $"New Draft incoming: {integrationEvent.DraftTitle}",
          HtmlBody = html
        },
        cancellationToken: cancellationToken);

    }
  }

  private sealed record RecipientRow(string EmailAddress, string FullName);
}
