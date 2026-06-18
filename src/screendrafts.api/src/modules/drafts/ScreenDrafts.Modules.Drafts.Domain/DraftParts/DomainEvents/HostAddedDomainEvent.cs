namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class HostAddedDomainEvent(Guid draftPartId, Guid hostId) : DomainEvent
{
  public Guid DraftPartId { get; } = draftPartId;

  public Guid HostId { get; } = hostId;
}
