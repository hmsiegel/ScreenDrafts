namespace ScreenDrafts.Modules.Movies.Features.Movies;

internal static class MovieLocations
{
  public static string ById(string imdbId) => $"movies/{imdbId}"; 
}
