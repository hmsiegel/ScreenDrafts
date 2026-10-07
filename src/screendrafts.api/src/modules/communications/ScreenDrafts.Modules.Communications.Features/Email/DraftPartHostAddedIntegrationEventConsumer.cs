namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class DraftPartHostAddedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService,
  IDateTimeProvider dateTimeProvider
) : IntegrationEventHandler<DraftPartHostAddedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DraftPartHostAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        ue.email_address AS EmailAddress,
        ue.full_name AS FullName
      FROM communications.user_emails ue
      WHERE ue.user_id = @UserId
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

    var recipient = await connection.QuerySingleOrDefaultAsync<RecipientRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { UserId = integrationEvent.RecipientUserId, EventId = integrationEvent.Id },
        cancellationToken: cancellationToken
      )
    );

    if (recipient is null)
    {
      return;
    }

    var html = EmailTemplates.HostAdded(
      recipientName: recipient.FullName,
      draftName: integrationEvent.DraftName,
      coHostNames: integrationEvent.CoHostNames
    );

    await _emailService.SendAsync(
      new EmailMessage
      {
        ToAddress = recipient.EmailAddress,
        ToName = recipient.FullName,
        Subject = $"You've been added as a host to {integrationEvent.DraftName}",
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

  private sealed record RecipientRow(string EmailAddress, string FullName);
}
