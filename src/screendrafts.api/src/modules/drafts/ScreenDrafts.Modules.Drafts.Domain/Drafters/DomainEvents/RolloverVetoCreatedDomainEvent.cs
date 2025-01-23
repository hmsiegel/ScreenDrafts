namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class RolloverVetoCreatedDomainEvent(Guid rolloverVetoId) : DomainEvent
{
  public Guid RolloverVetoId { get; init; } = rolloverVetoId;
}

