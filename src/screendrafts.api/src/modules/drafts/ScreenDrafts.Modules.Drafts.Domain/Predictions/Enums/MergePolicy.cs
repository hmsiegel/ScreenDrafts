namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Enums;

public sealed class MergePolicy(string name, int value) : SmartEnum<MergePolicy>(name, value)
{
  public static readonly MergePolicy UseHigherScore = new(nameof(UseHigherScore), 0);
  public static readonly MergePolicy UseBothScores = new(nameof(UseBothScores), 1);
}
