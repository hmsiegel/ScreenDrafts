namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ListPredictionSeasons;

internal sealed record PredictionSeasonDraftResponse
{
  public string DraftPublicId { get; init; } = default!;
  public string DraftPartPublicId { get; init; } = default!;

  /// <summary>
  /// "{Title}" for single-part drafts, "{Title} - Part {N}" for multi-part drafts.
  /// Display only; the client should not attempt to parse this.
  /// </summary>
  public string Label { get; init; } = default!;

  public int? EpisodeNumber { get; init; }
  public IReadOnlyList<PredictionSeasonDraftScoreResponse> Scores { get; init; } = [];
}
