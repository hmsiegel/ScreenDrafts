namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class RolloverVetoOverrideUsedDomainEvent(Guid rolloverVetoOverrideId, Guid toDraftId) : DomainEvent
{
  public Guid RolloverVetoOverrideId { get; init; } = rolloverVetoOverrideId;
  public Guid ToDraftId { get; init; } = toDraftId;
}
