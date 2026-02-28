namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class ReleaseAddedDomainEvent(Guid draftId, Guid partId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid PartId { get; init; } = partId;
}
