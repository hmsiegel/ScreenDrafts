namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

public static class MovieErrors
{
  public static readonly SDError InvalidMovieTitle = SDError.Problem(
    "InvalidMovieTitle",
    "The movie title is invalid."
  );
  public static readonly SDError InvalidPublicId = SDError.Problem(
    "InvalidPublicId",
    "The public ID is invalid."
  );

  public static SDError MovieAlreadyExists(string publicId) =>
    SDError.Problem(
      "MovieAlreadyExists",
      $"A movie with the public ID '{publicId}' already exists."
    );
}
