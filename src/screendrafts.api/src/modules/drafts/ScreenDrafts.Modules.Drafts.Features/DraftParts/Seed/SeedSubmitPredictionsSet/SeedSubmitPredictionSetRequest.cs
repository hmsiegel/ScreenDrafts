namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedSubmitPredictionsSet;

internal sealed record SeedSubmitPredictionSetRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  public string SeasonPublicId { get; init; } = default!;
  public string ContestantPublicId { get; init; } = default!;
  public string? SubmittedByPersonPublicId { get; init; }
  public int SourceKind { get; init; }
  public IReadOnlyList<SeedPredictionEntryRequest> Entries { get; init; } = [];
}
