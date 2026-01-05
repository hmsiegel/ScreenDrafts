namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Enums;

public sealed class PredictionMode(string name, int value) : SmartEnum<PredictionMode>(name, value)
{
  public static readonly PredictionMode UnorderedAll = new(nameof(UnorderedAll), 0);
  public static readonly PredictionMode UnorderedTopN = new(nameof(UnorderedTopN), 1);
  public static readonly PredictionMode OrderedTopN = new(nameof(OrderedTopN), 2);
  public static readonly PredictionMode OrderedAll = new(nameof(OrderedAll), 3);
}
