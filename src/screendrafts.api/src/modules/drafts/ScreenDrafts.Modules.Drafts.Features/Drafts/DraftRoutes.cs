namespace ScreenDrafts.Modules.Drafts.Features.Drafts;

internal static class DraftRoutes
{
  public const string Base = "/drafts";
  public const string Search = Base + "/search";
  public const string ById = Base + "/{publicId}";
  public const string Categories = Base + "/{publicId}/categories";
  public const string Category = Base + "/{publicId}/category";
  public const string CategoryById = Category + "/{categoryId}";
  public const string Campaign = Base + "/{publicId}/campaign";
  public const string Parts = Base + "/{publicId}/parts";
  public const string DraftStatus = Base + "/{publicId}/status";
  public const string DraftPartStatus = ById + "/parts/{partIndex:int}/status";
  public const string Episode = ById + "/episode";
  public const string Board = ById + "/board";
  public const string BoardItem = Board + "/{tmdbId}";
}
