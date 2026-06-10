namespace ScreenDrafts.Modules.Drafts.Domain.Attendances;

public sealed record DraftPartAttendanceId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;

  public static DraftPartAttendanceId CreateUnique() => new(Guid.NewGuid());

  public static DraftPartAttendanceId Create(Guid value) => new(value);
}
