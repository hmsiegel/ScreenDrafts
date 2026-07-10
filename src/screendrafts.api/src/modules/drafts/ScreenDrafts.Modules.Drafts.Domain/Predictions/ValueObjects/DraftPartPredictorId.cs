namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record DraftPartPredictorId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DraftPartPredictorId Create(Guid value) => new(value);

  public static DraftPartPredictorId FromString(string value) =>
    new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static DraftPartPredictorId CreateUnique() => new(Guid.NewGuid());
}
