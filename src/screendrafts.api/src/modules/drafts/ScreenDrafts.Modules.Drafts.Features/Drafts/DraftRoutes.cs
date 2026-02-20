namespace ScreenDrafts.Modules.Drafts.Features.Drafts;

internal static class DraftRoutes
{
  public const string Base = "/drafts";
  public const string ById = Base + "/{publicId}";
  public const string Categories = Base + "/{publicId}/categorise";
  public const string Campaign = Base + "/{publicId}/campaign";
  public const string DraftStatus = Base + "/{publicId}/status";
  public const string DraftPartStatus = ById + "/parts/{partIndex:int}/status";
}
