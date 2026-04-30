namespace ScreenDrafts.Modules.Drafts.Features.Predictions.Common;

internal sealed record PredictionSeasonSummaryResponse
{
  public required string PublicId { get; init; }
  public required int Number { get; init; }
  public required DateOnly StartDate { get; init; }
  public DateOnly? EndDate { get; init; }
  public required int? FirstEpisodeNumber { get; init; }
  public required int? LastEpisodeNumber { get; init; }
  public required int TargetPoints { get; init; }
  public required bool IsClosed { get; init; }
  public required IReadOnlyList<SeasonContestantStandingResponse> Standings { get; init; } = [];
}
