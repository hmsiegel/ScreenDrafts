namespace ScreenDrafts.Modules.Drafts.Domain.DraftPools;

public sealed class DraftPoolItem
{

  private DraftPoolItem(int tmdbId) => TmdbId = tmdbId;

  private DraftPoolItem()
  {
  }

  public int TmdbId { get; private set; }

  internal static DraftPoolItem Create(int tmdbId) => new(tmdbId);
}
