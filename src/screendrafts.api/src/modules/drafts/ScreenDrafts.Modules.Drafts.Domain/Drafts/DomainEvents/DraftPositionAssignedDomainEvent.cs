namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class DraftPositionAssignedDomainEvent(
  Guid draftId,
  Guid draftPositionId,
  Guid? drafterId = null,
  Guid? drafterTeamId = null) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DraftPositionId { get; init; } = draftPositionId;
  public Guid? DrafterId { get; init; } = drafterId;
  public Guid? DrafterTeamId { get; init; } = drafterTeamId;
}
