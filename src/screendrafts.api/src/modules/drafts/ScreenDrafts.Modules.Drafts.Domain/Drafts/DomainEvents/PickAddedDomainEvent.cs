namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class PickAddedDomainEvent(
  Guid draftId,
  int pickPosition,
  Guid movieId,
  Guid? drafterId,
  Guid? drafterTeamId,
  int playOrder,
  Guid draftPartId)
  : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;
  public Guid DraftPartId { get; init; } = draftPartId;
  public int PickPosition { get; init; } = pickPosition;
  public int PlayOrder { get; init; } = playOrder;
  public Guid MovieId { get; init; } = movieId;
  public Guid? DrafterId { get; init; } = drafterId;
  public Guid? DrafterTeamId { get; init; } = drafterTeamId;
}
