namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class RolloverVetoOverrideUsedDomainEvent(Ulid rolloverVetoOverrideId, Ulid toDraftId) : DomainEvent
{
  public Ulid RolloverVetoOverrideId { get; init; } = rolloverVetoOverrideId;
  public Ulid ToDraftId { get; init; } = toDraftId;
}
