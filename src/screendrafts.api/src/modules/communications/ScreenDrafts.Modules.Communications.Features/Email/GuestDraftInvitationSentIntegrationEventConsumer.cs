namespace ScreenDrafts.Modules.Communications.Features.Email;

// Unlike the other consumers, the recipient has no account yet, so there is no
// user_emails lookup: the event carries the address and name.
internal sealed class GuestDraftInvitationSentIntegrationEventConsumer(IEmailService emailService)
  : IntegrationEventHandler<GuestDraftInvitationSentIntegrationEvent>
{
  private readonly IEmailService _emailService = emailService;

  public override async Task Handle(
    GuestDraftInvitationSentIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var html = EmailTemplates.GuestDraftInvite(
      recipientName: integrationEvent.RecipientName,
      draftTitle: integrationEvent.DraftTitle,
      invitedByName: integrationEvent.InvitedByName,
      registerLink: integrationEvent.RegisterLink
    );

    await _emailService.SendAsync(
      new EmailMessage
      {
        ToAddress = integrationEvent.RecipientEmail,
        ToName = integrationEvent.RecipientName,
        Subject = $"You're invited to {integrationEvent.DraftTitle} on Screen Drafts",
        HtmlBody = html,
      },
      cancellationToken: cancellationToken
    );
  }
}
