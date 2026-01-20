namespace ScreenDrafts.Modules.Drafts.Features.Drafts;

internal static class DraftRoutes
{
  public const string Base = "/drafts";
  public const string ById = Base + "/{publicId:string}";
  public const string Categories = Base + "/{publicId:string}/categorise";
  public const string Campaign = Base + "/{publicId:string}/campaign";
  public const string DraftStatus = Base + "/{publicId:string}/status";
  public const string DraftPartStatus = ById + "/parts/{partIndex:int}/status";
}
