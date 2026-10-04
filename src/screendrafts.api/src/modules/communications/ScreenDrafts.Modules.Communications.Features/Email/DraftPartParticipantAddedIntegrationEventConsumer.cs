namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class DraftPartParticipantAddedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService,
  IDateTimeProvider dateTimeProvider
) : IntegrationEventHandler<DraftPartParticipantAddedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DraftPartParticipantAddedIntegrationEvent integrationEvent,
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
        parameters: new { UserId = integrationEvent.RecipientUserId },
        cancellationToken: cancellationToken
      )
    );

    if (recipient is null)
    {
      return;
    }

    var (subject, html) = integrationEvent.Kind switch
    {
      ParticipantAddedNotificationKind.Added => (
        $"You've been added as a participant to {integrationEvent.DraftName}",
        EmailTemplates.ParticipantAdded(
          recipientName: recipient.FullName,
          draftName: integrationEvent.DraftName,
          coParticipantNames: integrationEvent.CoParticipantNames
        )
      ),
      ParticipantAddedNotificationKind.CoParticipantNotification => (
        $"{integrationEvent.NewParticipantName} has joined {integrationEvent.DraftName}",
        EmailTemplates.CoParticipantJoined(
          recipientName: recipient.FullName,
          newParticipantName: integrationEvent.NewParticipantName,
          draftName: integrationEvent.DraftName,
          allParticipantNames: integrationEvent.CoParticipantNames
        )
      ),
      _ => throw new ScreenDraftsException(
        $"Unhandled {nameof(ParticipantAddedNotificationKind)}: {integrationEvent.Kind}"
      ),
    };

    await _emailService.SendAsync(
      new EmailMessage
      {
        ToAddress = recipient.EmailAddress,
        ToName = recipient.FullName,
        Subject = subject,
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
