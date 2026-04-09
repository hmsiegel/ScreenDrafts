namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Errors;

public static class PredictionErrors
{
  public static readonly SDError SetAlreadyLocked = SDError.Problem(
      "PredictionErrors.SetAlreadyLocked",
      "The prediction set is already locked.");

  public static readonly SDError SurrogatePrimarySetMismatch = SDError.Problem(
        "PredictionErrors.SurrogatePrimarySetMismatch",
        "The surrogate's primary set does not match the current set.");

  public static SDError TopNRequiredForPredictionMode(string name) =>
    SDError.Problem(
      "PredictionErrors.TopNRequiredForPredictionMode",
      $"TopN is required when PredictionMode is {name}.");

  public static SDError TopNNotAllowedForPredictionMode(string name) =>
    SDError.Problem(
      "PredictionErrors.TopNNotAllowedForPredictionMode",
      $"TopN is not allowed when PredictionMode is {name}.");

  public static SDError SeasonNotFound(string publicId) =>
  SDError.NotFound(
    "PredictionErrors.SeasonNotFound",
    $"Prediction season '{publicId}' was not found.");

  public static SDError ContestantNotFound(string publicId) =>
    SDError.NotFound(
      "PredictionErrors.ContestantNotFound",
      $"Prediction contestant '{publicId}' was not found.");

  public static SDError ContestantAlreadyExists(string personPublicId) =>
    SDError.Conflict(
      "PredictionErrors.ContestantAlreadyExists",
      $"A contestant already exists for person '{personPublicId}'.");

  public static SDError RulesNotFound(string draftPartId) =>
    SDError.NotFound(
      "PredictionErrors.RulesNotFound",
      $"No prediction rules found for draft part '{draftPartId}'.");

  public static SDError RulesAlreadyExist(string draftPartId) =>
    SDError.Conflict(
      "PredictionErrors.RulesAlreadyExist",
      $"Prediction rules already exist for draft part '{draftPartId}'.");

  public static SDError SetNotFound(string publicId) =>
    SDError.NotFound(
      "PredictionErrors.SetNotFound",
      $"Prediction set '{publicId}' was not found.");

  public static SDError SetAlreadyExists =>
    SDError.Conflict(
      "PredictionErrors.SetAlreadyExists",
      "This contestant already has a prediction set for this draft part.");

  public static SDError SetNotLocked =>
    SDError.Problem(
      "PredictionErrors.SetNotLocked",
      "This prediction set must be locked before it can be scored.");

  public static SDError DeadlinePassed =>
    SDError.Conflict(
      "PredictionErrors.DeadlinePassed",
      "The submission deadline for this draft part has passed.");

  public static SDError InvalidEntryCount(int required, int actual) =>
    SDError.Conflict(
      "PredictionErrors.InvalidEntryCount",
      $"Expected {required} predictions but received {actual}.");

  public static SDError SurrogateMismatch =>
    SDError.Conflict(
      "PredictionErrors.SurrogateMismatch",
      "The surrogate assignment does not belong to this prediction set.");

  public static readonly SDError SeasonAlreadyClosed = SDError.Conflict(
      "PredictionErrors.SeasonAlreadyClosed",
      "The prediction season is already closed.");

  public static SDError SurrogateSetNotFound(string publicId) =>
    SDError.NotFound(
      "PredictionErrors.SurrogateSetNotFound",
      $"Surrogate prediction set '{publicId}' was not found.");

  public static SDError AlreadyScored(string draftPartId) =>
    SDError.Conflict(
      "PredictionErrors.AlreadyScored",
      $"PredictionErrors.for draft part '{draftPartId}' have already been scored.");
}
