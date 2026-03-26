namespace ScreenDrafts.Modules.Movies.Domain.Medias.ValueObjects;

public sealed record MediaId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static MediaId CreateUnique() => new(Guid.NewGuid());

  public static MediaId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static MediaId Create(Guid value) => new(value);
}
