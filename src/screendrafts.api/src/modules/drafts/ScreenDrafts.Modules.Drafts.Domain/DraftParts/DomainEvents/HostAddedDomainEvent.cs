namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class HostAddedDomainEvent(Guid draftId, Guid hostId) : DomainEvent
{
  public Guid DraftId { get; } = draftId;

  public Guid HostId { get; } = hostId;
}
