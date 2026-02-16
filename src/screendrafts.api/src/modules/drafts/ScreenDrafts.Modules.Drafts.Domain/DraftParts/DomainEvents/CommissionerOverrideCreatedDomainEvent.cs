namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class CommissionerOverrideCreatedDomainEvent(Guid commissionerOverrideId, Guid pickId) : DomainEvent
{
  public Guid CommissionerOverrideId { get; } = commissionerOverrideId;
  public Guid PickId { get; } = pickId;
}
