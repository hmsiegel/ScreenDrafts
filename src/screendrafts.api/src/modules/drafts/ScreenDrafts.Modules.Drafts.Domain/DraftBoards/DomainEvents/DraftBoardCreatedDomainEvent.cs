namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards.DomainEvents;

public sealed class DraftBoardCreatedDomainEvent(
  Guid boardId,
  Guid draftId,
  Guid participant) : DomainEvent
{
  public Guid BoardId { get; } = boardId;
  public Guid DraftId { get; } = draftId;
  public Guid Participant { get; } = participant;
}


