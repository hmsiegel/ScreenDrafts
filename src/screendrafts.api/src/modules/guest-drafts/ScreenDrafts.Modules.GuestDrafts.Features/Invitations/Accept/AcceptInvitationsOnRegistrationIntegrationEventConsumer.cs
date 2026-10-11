namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Accept;

// ── Consumer ──────────────────────────────────────────────────────────────────
// Handler class name doubles as the idempotency key, so it must stay unique.
internal sealed class AcceptInvitationsOnRegistrationIntegrationEventConsumer(ISender sender)
  : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
  private readonly ISender _sender = sender;

  public override async Task Handle(
    UserRegisteredIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    await _sender.Send(
      new AcceptPendingInvitationsCommand
      {
        UserId = integrationEvent.UserId,
        Email = integrationEvent.Email,
        FirstName = integrationEvent.FirstName,
        LastName = integrationEvent.LastName,
      },
      cancellationToken
    );
  }
}
