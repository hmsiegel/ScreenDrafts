namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class CommissionerOverrideAppliedDomainEvent(Guid pickId, Guid commissionerOverrideId) : DomainEvent
{
  public Guid PickId { get; init; } = pickId;
  public Guid CommissionerOverrideId { get; init; } = commissionerOverrideId;
}
