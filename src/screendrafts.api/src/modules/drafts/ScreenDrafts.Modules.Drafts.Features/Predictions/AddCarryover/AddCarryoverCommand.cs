namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AddCarryover;

internal sealed record AddCarryoverCommand : ICommand
{
  public required string SeasonPublicId { get; init; }
  public required string ContestantPublicId { get; init; }
  public required int Points { get; init; }
  public required int Kind { get; init; }
  public string? Reason { get; init; }
}
