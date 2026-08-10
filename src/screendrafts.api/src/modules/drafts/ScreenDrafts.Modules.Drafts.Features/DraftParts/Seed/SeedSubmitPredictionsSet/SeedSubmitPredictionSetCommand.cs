namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedSubmitPredictionsSet;

internal sealed record SeedSubmitPredictionSetCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required string SeasonPublicId { get; init; }
  public required string ContestantPublicId { get; init; }
  public string? SubmittedByPersonPublicId { get; init; }
  public required int SourceKind { get; init; }
  public required IReadOnlyList<PredictionEntryDto> Entries { get; init; }
}
