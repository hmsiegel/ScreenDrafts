namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DraftPartParticipantAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid recipientUserId,
  string draftName,
  IReadOnlyList<string> coParticipantNames,
  ParticipantAddedNotificationKind kind,
  string newParticipantName)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid RecipientUserId { get; set; } = recipientUserId;
  public string DraftName { get; set; } = draftName;
  public ParticipantAddedNotificationKind Kind { get; set; } = kind;
  public string NewParticipantName { get; set; } = newParticipantName;
  public IReadOnlyList<string> CoParticipantNames { get; set; } = coParticipantNames;
}
