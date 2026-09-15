namespace ScreenDrafts.Modules.Communications.Features.Email;

internal sealed class EmailChangeConfirmationRequestedIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService
) : IntegrationEventHandler<EmailChangeConfirmationRequestedIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;

  public override async Task Handle(
    EmailChangeConfirmationRequestedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        email_address AS EmailAddress,
        full_name AS FullName
      FROM communications.user_emails
      WHERE user_id = @UserId
      """;

    // Deliberately looks up the CURRENT email on file — that's the point of
    // the confirmation step, proving the request came from whoever holds the
    // existing address. It doesn't reflect NewEmail until Confirm succeeds.
    var recipient = await connection.QuerySingleOrDefaultAsync<RecipientRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { UserId = integrationEvent.UserId },
        cancellationToken: cancellationToken
      )
    );

    if (recipient is null)
    {
      return;
    }

    var html = EmailTemplates.ConfirmEmailChange(
      recipientName: recipient.FullName,
      newEmail: integrationEvent.NewEmail,
      confirmationLink: integrationEvent.ConfirmationLink
    );

    await _emailService.SendAsync(
      new EmailMessage
      {
        ToAddress = recipient.EmailAddress,
        ToName = recipient.FullName,
        Subject = "Confirm your new ScreenDrafts email address",
        HtmlBody = html,
      },
      cancellationToken: cancellationToken
    );
  }

  private sealed record RecipientRow(string EmailAddress, string FullName);
}
