using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationEvents;

public sealed class GuestDraftInvitationSentIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string recipientEmail,
  string recipientName,
  string draftTitle,
  string invitedByName,
  string registerLink
) : IntegrationEvent(id, occurredOnUtc)
{
  public string RecipientEmail { get; set; } = recipientEmail;
  public string RecipientName { get; set; } = recipientName;
  public string DraftTitle { get; set; } = draftTitle;
  public string InvitedByName { get; set; } = invitedByName;
  public string RegisterLink { get; set; } = registerLink;
}
