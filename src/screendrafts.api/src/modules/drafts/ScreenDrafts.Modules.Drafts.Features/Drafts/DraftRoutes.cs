namespace ScreenDrafts.Modules.Drafts.Features.Drafts;

internal static partial class DraftRoutes
{
  public const string Base = "/drafts";
  public const string Search = Base + "/search";
  public const string ById = Base + "/{publicId}";
  public const string Categories = ById + "/categories";
  public const string Category = ById + "/category";
  public const string CategoryById = Category + "/{categoryId}";
  public const string Campaign = ById + "/campaign";
  public const string Parts = ById + "/parts";
  public const string DraftStatus = ById + "/status";
  public const string DraftPartStatus = ById + "/parts/{partIndex:int}/status";
  public const string Episode = ById + "/episode";
  public const string Board = ById + "/board";
  public const string BoardBulk = Board + "/bulk";
  public const string BoardItem = Board + "/{tmdbId}";
  public const string Pool = ById + "/pool";
  public const string PoolItem = Pool + "/items";
  public const string PoolBulk = Pool + "/bulk";
  public const string PoolItemById = PoolItem + "/{tmdbId}";
  public const string Latest = Base + "/latest";
  public const string Upcoming = Base + "/upcoming";
  public const string Image = ById + "/image";
  public const string Restore = ById + "/restore";
}
