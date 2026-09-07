namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafterTeams;

public sealed record GuestDrafterTeamId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static GuestDrafterTeamId CreateUnique() => new(Guid.NewGuid());

  public static GuestDrafterTeamId FromString(string value) =>
    new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static GuestDrafterTeamId Create(Guid value) => new(value);

  public static GuestDrafterTeamId Empty => new(Guid.Empty);
}
