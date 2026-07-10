namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed record PredictionEntryResponse
{
  public required int TmdbId { get; init; }
  public string? MediaPublicId { get; init; }
  public required string MediaTitle { get; init; }
  public int? OrderIndex { get; init; }
  public bool? IsCorrect { get; init; }
  public string? Notes { get; init; }
}
