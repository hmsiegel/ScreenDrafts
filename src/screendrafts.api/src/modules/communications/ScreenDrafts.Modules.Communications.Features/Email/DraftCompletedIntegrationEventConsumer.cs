using ScreenDrafts.Common.Application.Clock;

namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class DraftCompletedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService,
  IDateTimeProvider dateTimeProvider
) : IntegrationEventHandler<DraftCompletedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DraftCompletedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        ue.email_address AS EmailAddress,
        ue.full_name AS FullName
      FROM communications.user_emails ue
      WHERE (@IsPatreon = false OR ue.is_patreon = true)
        AND ue.email_address NOT ILIKE '%@screendrafts.fake'
        AND NOT EXISTS (
          SELECT 1
          FROM communications.email_deliveries d
          WHERE d.event_id = @EventId
                      AND d.email_address = ue.email_address)
      """;

    const string recordDeliverySql = """
      INSERT INTO communications.email_deliveries (event_id, email_address, sent_on_utc)
      VALUES (@EventId, @EmailAddress, @SentOnUtc)
      ON CONFLICT DO NOTHING
      """;

    var recipients = await connection.QueryAsync<RecipientRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { integrationEvent.IsPatreon, EventId = integrationEvent.Id },
        cancellationToken: cancellationToken
      )
    );

    foreach (var recipient in recipients)
    {
      var html = EmailTemplates.DraftCompleted(
        recipientName: recipient.FullName,
        draftName: integrationEvent.DraftTitle,
        isPatreon: integrationEvent.IsPatreon
      );

      await _emailService.SendAsync(
        new EmailMessage
        {
          ToAddress = recipient.EmailAddress,
          ToName = recipient.FullName,
          Subject = integrationEvent.IsPatreon
            ? $"[Patreon] Draft Completed: {integrationEvent.DraftTitle}"
            : $"Draft Completed: {integrationEvent.DraftTitle}",
          HtmlBody = html,
        },
        cancellationToken: cancellationToken
      );

      await connection.ExecuteAsync(
        new CommandDefinition(
          commandText: recordDeliverySql,
          parameters: new
          {
            EventId = integrationEvent.Id,
            recipient.EmailAddress,
            SentOnUtc = _dateTimeProvider.UtcNow,
          },
          cancellationToken: cancellationToken
        )
      );
    }
  }

  private sealed record RecipientRow(string EmailAddress, string FullName);
}
