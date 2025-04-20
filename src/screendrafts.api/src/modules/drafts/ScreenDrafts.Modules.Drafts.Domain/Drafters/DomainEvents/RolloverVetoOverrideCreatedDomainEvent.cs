namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.DomainEvents;

public sealed class RolloverVetoOverrideCreatedDomainEvent(
  Guid rolloverVetoOverrideId,
  Guid? drafterId,
  Guid? drafterTeamId,
  Guid fromDraftId) : DomainEvent
{
  public Guid RolloverVetoOverrideId { get; init; } = rolloverVetoOverrideId;
  public Guid? DrafterId { get; init; } = drafterId;
  public Guid? DrafterTeamId { get; init; } = drafterTeamId;
  public Guid FromDraftId { get; init; } = fromDraftId;
}
