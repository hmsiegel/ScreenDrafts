namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed record PredictionEntryDto
{
  public required string MediaPublicId { get; init; }
  public required string MediaTitle { get; init; }
  public int? OrderIndex { get; init; }
  public string? Notes { get; init; }
}
