using System.Globalization;

using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;

public sealed record UserId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static UserId CreateUnique() => new(Guid.NewGuid());

  public static UserId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static UserId Create(Guid value) => new(value);
}
