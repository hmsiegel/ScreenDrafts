namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedSubmitPredictionsSet;

internal sealed record SeedPredictionEntryRequest
{
  public int TmdbId { get; init; } = default!;
  public string MediaTitle { get; init; } = default!;
  public int? OrderIndex { get; init; }
  public string? Notes { get; init; }
}
