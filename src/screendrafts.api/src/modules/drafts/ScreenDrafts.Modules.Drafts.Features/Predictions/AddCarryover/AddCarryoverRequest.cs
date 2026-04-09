namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AddCarryover;

internal sealed record AddCarryoverRequest
{
  [FromRoute(Name = "seasonId")]
  public string SeasonPublicId { get; init; } = default!;

  public string ContestantPublicId { get; init; } = default!;
  public int Points { get; init; }
  public int Kind { get; init; }
  public string? Reason { get; init; }
}
