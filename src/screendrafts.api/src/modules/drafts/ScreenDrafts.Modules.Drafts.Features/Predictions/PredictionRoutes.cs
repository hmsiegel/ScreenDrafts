namespace ScreenDrafts.Modules.Drafts.Features.Predictions;

internal static class PredictionRoutes
{
  // Seasons
  public const string Seasons = "prediction-seasons";
  public const string GetCurrentSeason = Seasons + "/current";
  public const string GetSeasonStandings = Seasons + "/{publicId}/standings";
  public const string AddCarryover = Seasons + "/{publicId}/carryovers";
  public const string CloseSeason = Seasons + "/{publicId}/close";

  // Contestants
  public const string CreateContestant = "prediction-contestants";

  // Rules (draft-part scoped)
  public const string SetRules = "draft-parts/{draftPartId}/prediction-rules";

  // Sets (draft-part scoped)
  public const string SubmitSet = "draft-parts/{draftPartId}/predictions";
  public const string GetSets = "draft-parts/{draftPartId}/predictions";
  public const string LockSet = "draft-parts/{draftPartId}/predictions/{setId}/lock";
  public const string AssignSurrogate = "draft-parts/{draftPartId}/predictions/{setId}/surrogate";
  public const string ScoreSets = "draft-parts/{draftPartId}/predictions/score";
}
