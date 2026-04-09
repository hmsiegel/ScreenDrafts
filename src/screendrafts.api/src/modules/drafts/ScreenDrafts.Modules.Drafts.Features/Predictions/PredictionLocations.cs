namespace ScreenDrafts.Modules.Drafts.Features.Predictions;

internal static class PredictionLocations
{
  public static string SeasonById(string seasonPublicId) => $"/prediction-seasons/{seasonPublicId}";
  public static string ContestantById(string contestantPublicId) => $"/prediction-contestants/{contestantPublicId}";
}
