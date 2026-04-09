namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed record PredictionRulesSnapshot(
  PredictionMode Mode,
  int RequiredCount,
  int? TopN);
