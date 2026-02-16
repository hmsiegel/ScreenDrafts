using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class MovieErrors
{
  public static readonly SDError InvalidMovieTitle =
    SDError.Failure(
      "MovieErrors.InvalidMovieTitle",
      "The movie title is invalid.");

  public static readonly SDError InvalidImdbId =
    SDError.Failure(
      "MovieErrors.InvalidImdbId",
      "The IMDb ID is invalid.");

  public static readonly SDError VersionNotAllowedByPolicy =
        SDError.Failure(
      "MovieErrors.VersionNotAllowedByPolicy",
      "The specified version is not allowed by the draft policy.");

  public static readonly SDError VersionNameTooLong =
    SDError.Failure(
      "MovieErrors.VersionNameTooLong",
      "The specified version name is too long.");

  public static readonly SDError UnknownVersionForMovie =
    SDError.Failure(
      "MovieErrors.UnknownVersionForMovie",
      "The specified version is unknown for the movie.");

  public static readonly SDError VersionIsRequiredByPolicy =
    SDError.Failure(
      "MovieErrors.VersionIsRequiredByPolicy",
      "A version is required by the draft policy.");

  public static readonly SDError VersionDoesNotMatchRequiredPolicy =
    SDError.Failure(
      "MovieErrors.VersionDoesNotMatchRequiredPolicy",
      "The specified version does not match the required draft policy.");

  public static readonly SDError MovieIdRequired =
    SDError.Failure(
      "MovieErrors.MovieIdRequired",
      "A movie ID is required.");

  public static SDError MovieAlreadyExists(string imdbId) =>
    SDError.Conflict(
      "Drafts.MovieAlreadyExists",
      $"Movie with IMDB id {imdbId} already exists.");
}
