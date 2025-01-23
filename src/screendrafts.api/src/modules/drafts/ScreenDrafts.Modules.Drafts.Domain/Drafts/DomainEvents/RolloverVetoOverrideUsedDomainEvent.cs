namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class RolloverVetoOverrideUsedDomainEvent(Guid rolloverVetoOverrideId, Guid toDraftId) : DomainEvent
{
  public Guid RolloverVetoOverrideId { get; init; } = rolloverVetoOverrideId;
  public Guid ToDraftId { get; init; } = toDraftId;
}
