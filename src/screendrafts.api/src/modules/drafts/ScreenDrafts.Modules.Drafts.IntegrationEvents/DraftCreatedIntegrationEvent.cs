namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed class DraftCreatedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid draftId,
  string draftTitle,
  bool isPatreon)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DraftId { get; set; } = draftId;
  public string DraftTitle { get; set; } = draftTitle;
  public bool IsPatreon { get; set; } = isPatreon;
}
