namespace ScreenDrafts.Modules.GuestDrafts.Domain.Invitations;

public sealed record InvitationId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static InvitationId CreateUnique() => new(Guid.NewGuid());

  public static InvitationId FromString(string value) =>
    new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static InvitationId Create(Guid value) => new(value);

  public static InvitationId Empty => new(Guid.Empty);
}
