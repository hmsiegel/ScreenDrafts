namespace ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;

public sealed record DrafterTeamId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static DrafterTeamId CreateUnique() => new(Guid.NewGuid());

  public static DrafterTeamId FromString(string value) =>
    new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static DrafterTeamId Create(Guid value) => new(value);

  public static DrafterTeamId Empty => new(Guid.Empty);
}
