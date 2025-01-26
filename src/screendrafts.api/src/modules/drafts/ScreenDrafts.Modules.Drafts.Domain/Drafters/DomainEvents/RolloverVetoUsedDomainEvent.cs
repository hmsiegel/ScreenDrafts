namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class RolloverVetoUsedDomainEvent(Guid rolloverVetoId, Guid toDraftId) : DomainEvent
{
  public Guid RolloverVetoId { get; init; } = rolloverVetoId;

  public Guid ToDraftId { get; init; } = toDraftId;
}

