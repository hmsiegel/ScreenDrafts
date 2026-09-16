namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed record PredictionEntryDto
{
  public required int TmdbId { get; init; }
  public required string MediaTitle { get; init; }
  public int? OrderIndex { get; init; }
  public string? Notes { get; init; }

  /// <summary>
  /// Set only for a TV-episode entry. The frontend resolves/imports the
  /// episode via importAndResolveEpisode (same call pick-source-panel makes)
  /// before it ever reaches this DTO, so by the time this is populated the
  /// Movie row is already guaranteed to exist — the handler skips its
  /// existence-check-and-fetch-event dance whenever this is set. Null for a
  /// movie entry, unchanged from today.
  /// </summary>
  public string? MediaPublicId { get; init; }
}
