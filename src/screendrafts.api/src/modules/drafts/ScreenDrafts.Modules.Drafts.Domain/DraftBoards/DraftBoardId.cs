namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards;

public sealed record DraftBoardId(Guid Value) : AggregateRootId<Guid>
{
  public override Guid Value { get; protected set; } = Value;
  public static DraftBoardId CreateUnique() => new(Guid.NewGuid());
  public static DraftBoardId Create(Guid value) => new(value);
}
