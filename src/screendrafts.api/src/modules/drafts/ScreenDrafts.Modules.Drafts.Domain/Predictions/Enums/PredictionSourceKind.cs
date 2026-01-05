namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Enums;

public sealed class PredictionSourceKind(
  string name,
  int value)
  : SmartEnum<PredictionSourceKind>(name, value)
{
  public static readonly PredictionSourceKind UI = new("UI", 0);
  public static readonly PredictionSourceKind TextUpload = new("TextUpload", 1);
  public static readonly PredictionSourceKind API = new("API", 2);
}
