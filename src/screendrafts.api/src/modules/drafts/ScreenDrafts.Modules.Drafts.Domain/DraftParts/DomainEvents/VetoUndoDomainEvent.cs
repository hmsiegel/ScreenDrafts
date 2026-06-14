namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class VetoUndoDomainEvent(
  Guid DraftPartId,
  string DraftPartPublicId,
  Guid PickId,
  int PlayOrder,
  int TmdbId,
  Guid DraftId,
  string DraftPublicId,
  string? movieTitle
) : DomainEvent
{
  public Guid DraftPartId { get; init; } = DraftPartId;
  public string DraftPartPublicId { get; init; } = DraftPartPublicId;
  public Guid PickId { get; init; } = PickId;
  public int PlayOrder { get; init; } = PlayOrder;
  public int TmdbId { get; init; } = TmdbId;
  public Guid DraftId { get; init; } = DraftId;
  public string DraftPublicId { get; init; } = DraftPublicId;
  public string? MovieTitle { get; init; } = movieTitle;
}
