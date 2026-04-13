namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class HostAddedDomainEvent(Guid draftId, Guid hostId) : DomainEvent
{
  public Guid DraftPartId { get; } = draftId;

  public Guid HostId { get; } = hostId;
}
