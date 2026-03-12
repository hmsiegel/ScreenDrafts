namespace ScreenDrafts.Modules.Drafts.Domain.DraftPools;

public sealed class MovieAddedToDraftPoolDomainEvent(Guid draftPoolId, Guid draftId, int tmdbId) : DomainEvent
{
  public Guid DraftPoolId { get; init; } = draftPoolId;
  public Guid DraftId { get; init; } = draftId;
  public int TmdbId { get; init; } = tmdbId;
}
