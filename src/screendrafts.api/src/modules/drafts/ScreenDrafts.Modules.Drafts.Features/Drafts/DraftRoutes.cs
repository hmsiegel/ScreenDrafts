namespace ScreenDrafts.Modules.Drafts.Features.Drafts;

internal static class DraftRoutes
{
  public const string Base = "/drafts";
  public const string Search = Base + "/search";
  public const string ById = Base + "/{draftId}";
  public const string Categories = Base + "/{draftId}/categories";
  public const string Category = Base + "/{draftId}/category";
  public const string CategoryById = Category + "/{categoryId}";
  public const string Campaign = Base + "/{draftId}/campaign";
  public const string Parts = Base + "/{draftId}/parts";
  public const string DraftStatus = Base + "/{draftId}/status";
  public const string DraftPartStatus = ById + "/parts/{partIndex:int}/status";
  public const string Episode = ById + "/episode";
  public const string Board = ById + "/board";
  public const string BoardBulk = Board + "/bulk";
  public const string BoardItem = Board + "/{tmdbId}";
  public const string Pool = ById + "/pool";
  public const string PoolItem = Pool + "/items";
  public const string PoolBulk = Pool + "/bulk";
  public const string Latest = Base + "/latest";
  public const string Upcoming = Base + "/upcoming";
}
