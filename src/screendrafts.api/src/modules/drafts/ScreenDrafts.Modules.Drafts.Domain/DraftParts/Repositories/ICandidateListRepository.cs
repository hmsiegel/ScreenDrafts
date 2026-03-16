namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;

public interface ICandidateListRepository : IRepository<CandidateListEntry, CandidateListEntryId>
{
  Task<CandidateListEntry?> FindByTmdbIdAsync(DraftPartId draftPartId, int tmdbId, CancellationToken cancellationToken = default);

  Task<HashSet<int>> GetExistingTmdbIdsAsync(DraftPartId draftPartId, CancellationToken cancellationToken = default);

  Task<Guid?> FindMovieByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken = default);

  Task<Dictionary<int, Guid>> FindMoviesByTmdbIdsAsync(IReadOnlyList<int> tmdbIds, CancellationToken cancellationToken = default);

  Task<List<CandidateListEntry>> GetPendingEntriesByTmdbIdAsync(int tmdbId, CancellationToken cancellationToken = default);

  Task AddRange(IReadOnlyList<CandidateListEntry> entries);

  void UpdateRange(IReadOnlyList<CandidateListEntry> entries);
}
