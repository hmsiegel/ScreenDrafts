namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record HostId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static HostId CreateUnique() => new(Guid.NewGuid());

  public static HostId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static HostId Create(Guid value) => new(value);
}
