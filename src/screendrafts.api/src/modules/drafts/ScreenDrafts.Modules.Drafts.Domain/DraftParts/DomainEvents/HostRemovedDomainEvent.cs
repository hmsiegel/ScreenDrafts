namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class HostRemovedDomainEvent(Guid draftId, Guid hostId) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid HostId { get; init; } = hostId;
}
