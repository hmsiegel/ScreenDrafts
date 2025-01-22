namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class RolloverVetoOverrideCreatedDomainEvent(Ulid rolloverVetoOverrideId) : DomainEvent
{
  public Ulid RolloverVetoOverrideId { get; init; } = rolloverVetoOverrideId;
}
