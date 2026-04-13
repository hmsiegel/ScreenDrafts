namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class DraftPartHostAddedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService)
    : IntegrationEventHandler<DraftPartHostAddedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;

  public override async Task Handle(DraftPartHostAddedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
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

    var recipient = await connection.QuerySingleAsync<RecipientRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { UserId = integrationEvent.RecipientUserId },
        cancellationToken: cancellationToken));

    if (recipient is null)
    {
      return;
    }

    var html = EmailTemplates.HostAdded(
      recipientName: recipient.FullName,
      draftName: integrationEvent.DraftName,
      coHostNames: integrationEvent.CoHostNames);

    await _emailService.SendAsync(
      new EmailMessage
      {
        ToAddress = recipient.EmailAddress,
        ToName = recipient.FullName,
        Subject = $"You've been added as a host to {integrationEvent.DraftName}",
        HtmlBody = html
      },
      cancellationToken: cancellationToken);
  }

  private sealed record RecipientRow(string EmailAddress, string FullName);
}
