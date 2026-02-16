using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static partial class DraftErrors
{
  public static SDError PickPositionAlreadyTaken(int position) =>
   SDError.Conflict(
     "Drafts.PickPositionAlreadyTaken",
     $"Pick position {position} is already taken.");

  public static readonly SDError PickPositionIsOutOfRange = SDError.Conflict(
    "Drafts.PickPositionIsOutOfRange",
    "Pick position is out of range.");

  public static readonly SDError DraftMustHaveAtLeastFivePicks = SDError.Problem(
    "Drafts.DraftMustHaveAtLeastFivePicks",
    "Draft must have at least five picks.");

  public static readonly SDError InvalidPickPosition = SDError.Problem(
    "Drafts.InvalidPickPosition",
    "Invalid pick position.");

  public static SDError MovieNotFound(Guid movieId) =>
    SDError.NotFound(
      "Drafts.MovieNotFound",
      $"Movie with id {movieId} was not found.");

  public static SDError MovieAlreadyAdded(Guid movieId) =>
    SDError.Conflict(
      "Drafts.MovieAlreadyAdded",
      $"Movie with id {movieId} is already added to the draft.");

  public static SDError MovieAlreadyPicked(Guid movieId) =>
    SDError.Conflict(
      "Drafts.MovieAlreadyPicked",
      $"Movie with id {movieId} is already picked.");

  public static SDError InvalidNumberOfPicks(int totalPicks, int numberOfDraftPositionPicks) =>
    SDError.Problem(
      "Drafts.InvalidNumberOfPicks",
      $"Total picks {totalPicks} does not match the number of picks {numberOfDraftPositionPicks}.");

  public static readonly SDError CannotAddPickIfDraftIsNotStarted = SDError.Problem(
    "Drafts.CannotAddPickIfDraftIsNotStarted",
    "Cannot add a pick if the draft is not started.");

  public static readonly SDError CannotAddPickIfDraftIsCompleted = SDError.Problem(
    "Drafts.CannotAddPickIfDraftIsCompleted",
    "Cannot add a pick if the draft is completed.");

  public static readonly SDError CannotAddPickIfDraftIsPaused = SDError.Problem(
    "Drafts.CannotAddPickIfDraftIsPaused",
    "Cannot add a pick if the draft is paused.");

  public static readonly SDError PickPlayOrderIsOutOfRange = SDError.Conflict(
    "Drafts.PickPlayOrderIsOutOfRange",
    "Pick play order is out of range.");

  public static readonly SDError PicksNotFound = SDError.NotFound(
    "Drafts.PicksNotFound",
    "No draft picks were found.");

  public static SDError PickNotFound(Guid id) =>
    SDError.NotFound(
      "Drafts.PickNotFound",
      $"Pick with id {id} was not found.");
}
