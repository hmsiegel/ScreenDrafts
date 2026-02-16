namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class DraftPositionCreatedDomainEvent(DraftPositionId draftPositionId) : DomainEvent
{
  public DraftPositionId DraftPositionId { get; } = draftPositionId;
}
