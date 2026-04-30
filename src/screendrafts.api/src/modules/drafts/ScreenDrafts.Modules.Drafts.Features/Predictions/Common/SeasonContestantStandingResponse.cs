namespace ScreenDrafts.Modules.Drafts.Features.Predictions.Common;

internal sealed record SeasonContestantStandingResponse
{
  public required string ContestantPublicId { get; init; }
  public required string DisplayName { get; init; }
  public required int Points { get; init; }
  public required bool HasCrossedTarget { get; init; }
  public DateTime? FirstCrossedTargetAtUtc { get; init; }
}
