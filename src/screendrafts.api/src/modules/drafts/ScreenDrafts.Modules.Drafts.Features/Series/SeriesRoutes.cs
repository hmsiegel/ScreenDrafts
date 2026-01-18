namespace ScreenDrafts.Modules.Drafts.Features.Series;

internal static class SeriesRoutes
{
  public const string Series = "/series";
  public const string ById = Series + "/{publicId:string}";
  public const string Metadata = Series + "/metadata";
}
