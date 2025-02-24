namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

public interface ITriviaResultsRepository : IRepository
{
  void Add(TriviaResult triviaResult);

  Task<TriviaResult?> GetByDrafterIdAsync(DrafterId drafterId, DraftId draftId);
}
