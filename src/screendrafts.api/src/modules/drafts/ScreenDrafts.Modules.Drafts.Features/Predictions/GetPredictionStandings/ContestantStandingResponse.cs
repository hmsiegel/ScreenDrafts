namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetPredictionStandings;

internal sealed record ContestantStandingResponse
{
  public string ContestantPublicId { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
  public int Points { get; init; }
  public int CarryoverPoints { get; init; }
  public int TotalPoints { get; init; }
  public bool HasCrossedTarget { get; init; }
  public DateTime? FirstCrossedTargetAtUtc { get; init; }
}
