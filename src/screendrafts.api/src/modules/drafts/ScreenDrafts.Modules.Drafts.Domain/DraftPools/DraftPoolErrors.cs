namespace ScreenDrafts.Modules.Drafts.Domain.DraftPools;

public static class DraftPoolErrors
{
  public static readonly SDError InvalidPublicId = SDError.Failure(
    "DraftPool.InvalidPublicId",
    "The public ID must not be null, empty, or whitespace.");

  public static readonly SDError PoolIsLocked = SDError.Failure(
    "DraftPool.PoolIsLocked",
    "Cannot add a movie to a locked draft pool.");

  public static readonly SDError PoolAlreadyExists = SDError.Conflict(
    "DraftPool.PoolAlreadyExists",
    "A draft pool already exists for this draft.");

  public static readonly SDError DraftHasPool = SDError.Failure(
    "DraftPool.DraftHasPool",
    "The draft already has a pool.");

  public static SDError MovieAlreadyExists(int tmdbId) => SDError.Failure(
      "DraftPool.MovieAlreadyExists",
      $"The movie with TMDB ID {tmdbId} is already in the draft pool.");

  public static SDError NotFound(string publicId) => SDError.NotFound(
    "DraftPool.NotFound",
    $"No draft pool found for draft with public ID '{publicId}'.");

}
