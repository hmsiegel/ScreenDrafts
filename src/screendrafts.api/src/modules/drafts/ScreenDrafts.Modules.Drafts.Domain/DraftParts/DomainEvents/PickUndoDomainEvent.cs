namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class PickUndoDomainEvent(
  Guid DraftPartId,
  string DraftPartPublicId,
  int PlayOrder,
  int BoardPosition,
  int TmdbId,
  string MovieTitle,
  Guid DraftId,
  string DraftPublicId
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = DraftPartId;
  public string DraftPartPublicId { get; init; } = DraftPartPublicId;
  public int PlayOrder { get; init; } = PlayOrder;
  public int BoardPosition { get; init; } = BoardPosition;
  public int TmdbId { get; init; } = TmdbId;
  public string MovieTitle { get; init; } = MovieTitle;
  public Guid DraftId { get; init; } = DraftId;
  public string DraftPublicId { get; init; } = DraftPublicId;
}
