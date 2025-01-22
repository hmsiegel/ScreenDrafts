namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class RolloverVetoCreatedDomainEvent(Ulid rolloverVetoId) : DomainEvent
{
  public Ulid RolloverVetoId { get; init; } = rolloverVetoId;
}

