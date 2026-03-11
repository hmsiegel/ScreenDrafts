namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards;

public interface IDraftBoardRepository : IRepository<DraftBoard, DraftBoardId>
{
  Task<DraftBoard?> GetByDraftAndParticipantAsync(DraftId draftId, Participant participantId, CancellationToken cancellationToken);
}
