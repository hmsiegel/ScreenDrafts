namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPositionAssignedDomainEvent(
  Guid draftPartId,
  Guid draftPositionId,
  Guid? drafterId = null,
  Guid? drafterTeamId = null) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public Guid DraftPositionId { get; init; } = draftPositionId;
  public Guid? DrafterId { get; init; } = drafterId;
  public Guid? DrafterTeamId { get; init; } = drafterTeamId;
}
