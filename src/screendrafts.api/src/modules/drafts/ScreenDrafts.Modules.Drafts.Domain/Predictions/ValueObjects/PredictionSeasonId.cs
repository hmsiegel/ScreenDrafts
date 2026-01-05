namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

public sealed record PredictionSeasonId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static PredictionSeasonId CreateUnique() => new(Guid.NewGuid());

  public static PredictionSeasonId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static PredictionSeasonId Create(Guid value) => new(value);

  public static PredictionSeasonId Empty => new(Guid.Empty);
}
