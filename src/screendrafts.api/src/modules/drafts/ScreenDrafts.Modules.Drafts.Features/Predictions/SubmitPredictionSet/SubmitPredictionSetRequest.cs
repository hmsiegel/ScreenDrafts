namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed record SubmitPredictionSetRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  public string SeasonPublicId { get; init; } = default!;
  public string ContestantPublicId { get; init; } = default!;
  public string? SubmittedByPersonPublicId { get; init; }
  public int SourceKind { get; init; }
  public IReadOnlyList<PredictionEntryRequest> Entries { get; init; } = [];
}
