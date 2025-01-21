namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class HostAddedDomainEvent(Ulid draftId, Ulid hostId) : DomainEvent
{
  public Ulid DraftId { get; } = draftId;

  public Ulid HostId { get; } = hostId;
}
