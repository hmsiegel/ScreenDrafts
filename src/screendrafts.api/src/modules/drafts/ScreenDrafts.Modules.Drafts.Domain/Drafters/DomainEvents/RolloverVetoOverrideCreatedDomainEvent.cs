namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class RolloverVetoOverrideCreatedDomainEvent(Guid rolloverVetoOverrideId) : DomainEvent
{
  public Guid RolloverVetoOverrideId { get; init; } = rolloverVetoOverrideId;
}
