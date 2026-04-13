using ScreenDrafts.Common.Abstractions.Exceptions;

namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class DraftPartParticipantAddedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService)
    : IntegrationEventHandler<DraftPartParticipantAddedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;

  public override async Task Handle(
    DraftPartParticipantAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      SELECT
        email_address AS EmailAddress,
        full_name AS FullName
      FROM communications.user_emails
      WHERE user_id = @UserId
      """;

    var recipient = await connection.QuerySingleOrDefaultAsync<RecipientRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { UserId = integrationEvent.RecipientUserId },
        cancellationToken: cancellationToken));

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
          coParticipantNames: integrationEvent.CoParticipantNames)
      ),
      ParticipantAddedNotificationKind.CoParticipantNotification => (
        $"{integrationEvent.NewParticipantName} has joined {integrationEvent.DraftName}",
        EmailTemplates.CoParticipantJoined(
          recipientName: recipient.FullName,
          newParticipantName: integrationEvent.NewParticipantName,
          draftName: integrationEvent.DraftName,
          allParticipantNames: integrationEvent.CoParticipantNames)
      ),
      _ => throw new ScreenDraftsException($"Unhandled {nameof(ParticipantAddedNotificationKind)}: {integrationEvent.Kind}")
    };

    await _emailService.SendAsync(
      new EmailMessage
      {
        ToAddress = recipient.EmailAddress,
        ToName = recipient.FullName,
        Subject = subject,
        HtmlBody = html
      },
      cancellationToken: cancellationToken);
  }

  private sealed record RecipientRow(string EmailAddress, string FullName);
}
