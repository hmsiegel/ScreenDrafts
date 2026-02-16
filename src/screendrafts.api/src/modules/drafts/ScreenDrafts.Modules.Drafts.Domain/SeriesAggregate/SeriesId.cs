namespace ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;

public sealed record SeriesId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static SeriesId CreateUnique() => new(Guid.NewGuid());

  public static SeriesId FromString(string value) => new(Guid.Parse(value));

  public static SeriesId Create(Guid value) => new(value);
}
