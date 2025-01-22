namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class RolloverVetoUsedDomainEvent(Ulid rolloverVetoId, Ulid toDraftId) : DomainEvent
{
  public Ulid RolloverVetoId { get; init; } = rolloverVetoId;

  public Ulid ToDraftId { get; init; } = toDraftId;
}

