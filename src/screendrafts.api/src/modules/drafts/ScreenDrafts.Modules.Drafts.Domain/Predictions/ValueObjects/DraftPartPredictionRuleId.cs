namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record DraftPartPredictionRuleId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DraftPartPredictionRuleId CreateUnique() => new(Guid.NewGuid());
  public override string ToString() => Value.ToString();
  public static DraftPartPredictionRuleId Create(Guid value) => new(value);
  public static DraftPartPredictionRuleId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
}
