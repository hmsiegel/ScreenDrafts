namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class RolloverVetoOverrideCreatedDomainEvent(Guid rolloverVetoOverrideId) : DomainEvent
{
  public Guid RolloverVetoOverrideId { get; init; } = rolloverVetoOverrideId;
}
