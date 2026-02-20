namespace ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

public sealed record DrafterTeamId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DrafterTeamId CreateUnique() => new(Guid.NewGuid());

  public static DrafterTeamId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static DrafterTeamId Create(Guid value) => new(value);
}
