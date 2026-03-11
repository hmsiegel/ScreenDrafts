namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards.DomainEvents;

public sealed class MovieRemovedFromDraftBoardDomainEvent(
    Guid boardId,
    Guid draftId,
    Guid participantId) : DomainEvent
{
  public Guid BoardId { get; } = boardId;
  public Guid DraftId { get; } = draftId;
  public Guid ParticipantId { get; } = participantId;
}


