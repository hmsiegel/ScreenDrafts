namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed record PredictionEntryRequest
{
  public string MediaPublicId { get; init; } = default!;
  public string MediaTitle { get; init; } = default!;
  public int? OrderIndex { get; init; }
  public string? Notes { get; init; }

}
