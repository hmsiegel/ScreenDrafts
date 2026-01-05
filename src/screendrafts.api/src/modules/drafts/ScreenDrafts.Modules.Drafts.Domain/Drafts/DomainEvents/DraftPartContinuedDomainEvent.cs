namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPartContinuedDomainEvent(Guid draftPartId) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
}
