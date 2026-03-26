namespace ScreenDrafts.Modules.Movies.Features.Movies;

internal static class MoviesRoutes
{
  internal const string Base = "/media";
  internal const string GetMedia = Base + "/{publicId}";
  internal const string GetMediaSummary = Base + "/{publicId}/summary";

  internal const string MediaSearch = "/draft-parts/{draftPartId}/media/search";
}
