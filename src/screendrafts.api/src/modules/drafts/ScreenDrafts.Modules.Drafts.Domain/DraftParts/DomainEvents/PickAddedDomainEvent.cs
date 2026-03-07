namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class PickAddedDomainEvent(
  Guid draftPartId,
  string imdbId,
  string movieTitle) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public string ImdbId { get; init; } = imdbId;
  public string MovieTitle { get; init; } = movieTitle;
}
