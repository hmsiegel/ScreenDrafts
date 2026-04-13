namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DraftPartHostAddedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid recipientUserId,
  string draftName,
  IReadOnlyList<string> coHostNames)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid RecipientUserId { get; set; } = recipientUserId;
  public string DraftName { get; set; } = draftName;
  public IReadOnlyList<string> CoHostNames { get; set; } = coHostNames;
}
