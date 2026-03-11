namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards;

public static class DraftBoardErrors
{
  public static readonly SDError InvalidTmdbId = SDError.Failure(
    "DraftBoardErrors.InvalidTmdbId",
    "The provided TMDB ID is invalid.");

  public static readonly SDError InvalidPublicId = SDError.Failure(
        "DraftBoardErrors.InvalidPublicId",
        "The provided public ID is invalid.");

  public static SDError MovieAlreadyOnTheBoard(int tmdbId) =>
    SDError.Failure(
      "DraftBoardErrors.MovieAlreadyOnTheBoard",
      $"The movie with TMDB ID {tmdbId} is already on the board.");


  public static SDError MovieNotFoundOnTheBoard(int tmdbId) =>
    SDError.Failure(
      "DraftBoardErrors.MovieNotFoundOnTheBoard",
      $"The movie with TMDB ID {tmdbId} was not found on the board.");

  public static SDError NotFoundForParticipant(Guid userId) =>
    SDError.Failure(
      "DraftBoardErrors.NotFoundForParticipant",
      $"No draft board was found for participant with user ID {userId}.");


  public static SDError ParticipantNotFound(string userPublicId) =>
    SDError.Failure(
      "DraftBoardErrors.ParticipantNotFound",
      $"The participant with public ID {userPublicId} was not found.");
}
