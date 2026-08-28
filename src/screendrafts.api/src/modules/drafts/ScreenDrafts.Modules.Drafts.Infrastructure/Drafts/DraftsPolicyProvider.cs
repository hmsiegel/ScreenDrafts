namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftsPolicyProvider(DraftsDbContext dbContext, ICacheService cacheService)
  : IDraftPolicyProvider
{
  private readonly DraftsDbContext _dbContext = dbContext;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<DraftPolicySnapshot?> GetDraftPolicyAsync(
    DraftId draftId,
    CancellationToken cancellationToken
  )
  {
    var cacheKey = $"drafts:draft-policy:{draftId.Value}";

    if (_cacheService.TryGetValue(cacheKey, out DraftPolicySnapshot? cachedSnapshot))
    {
      return cachedSnapshot;
    }

    var fungibleTokenName = await _dbContext
      .Drafts.Where(d => d.Id == draftId)
      .Select(d => d.FungibleTokenName)
      .FirstOrDefaultAsync(cancellationToken);

    var snapshot = new DraftPolicySnapshot(fungibleTokenName);

    await _cacheService.SetAsync(cacheKey, snapshot, cancellationToken: cancellationToken);

    return snapshot;
  }
}
