namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class PickAddedDomainEvent(Guid draftPartId) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
}
