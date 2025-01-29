namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class MovieErrors
{
  public static readonly SDError InvalidMovieTitle =
    SDError.Failure(
      "MovieErrors.InvalidMovieTitle",
      "The movie title is invalid.");
}
