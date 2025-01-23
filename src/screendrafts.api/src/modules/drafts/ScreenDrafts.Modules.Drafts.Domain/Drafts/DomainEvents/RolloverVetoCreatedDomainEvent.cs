namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class RolloverVetoCreatedDomainEvent(Guid rolloverVetoId) : DomainEvent
{
  public Guid RolloverVetoId { get; init; } = rolloverVetoId;
}

