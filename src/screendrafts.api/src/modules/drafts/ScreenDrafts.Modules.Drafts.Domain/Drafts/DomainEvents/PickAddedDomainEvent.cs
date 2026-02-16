namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class PickAddedDomainEvent(
  Guid draftId,
  Guid movieId,
  Guid draftPartId,
  Guid drafterId,
  Guid drafterTeamId,
  int pickPosition,
  int playOrder)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DraftPartId { get; init; } = draftPartId;
  public Guid DrafterId { get; } = drafterId;
  public Guid DrafterTeamId { get; } = drafterTeamId;
  public int PickPosition { get; init; } = pickPosition;
  public int PlayOrder { get; init; } = playOrder;
  public Guid MovieId { get; init; } = movieId;
}
