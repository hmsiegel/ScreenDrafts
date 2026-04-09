namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictions;

internal sealed record DraftPartPredictionResponse
{
  public required string PublicId { get; init; }
  public required string ContestantPublicId { get; init; }
  public required string ContestantDisplayName { get; init; }
  public required DateTime SubmittedAtUtc { get; init; }
  public required string SourceKind { get; init; }
  public required bool IsLocked { get; init; }
  public DateTime? LockedAtUtc { get; init; }
  public required IReadOnlyList<PredictionEntryResponse> Entries { get; init; }
  public PredictionResultResponse? Result { get; init; }
}
