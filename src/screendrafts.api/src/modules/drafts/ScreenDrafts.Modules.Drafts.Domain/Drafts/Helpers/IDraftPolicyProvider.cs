namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Helpers;

/// <summary>
/// Infrastructure service for loading a small, stable slice of Draft-level policy data
/// without loading the full Draft aggregate and its Parts/Picks graph. Sibling to
/// ISeriesPolicyProvider, same reasoning: handlers operating on a DraftPart loaded via
/// IDraftPartRepository don't have a Draft navigation available, but sometimes need a
/// piece of data that only lives on Draft.
///
/// Deliberately returns a narrow snapshot rather than the full Draft entity. Unlike
/// Series, Draft is a large, frequently-mutating aggregate (status, parts, picks) —
/// caching the whole thing for any meaningful duration risks serving stale gameplay
/// state. FungibleTokenName is set once at draft setup and doesn't change during play,
/// so it's safe to cache on its own.
/// </summary>
public interface IDraftPolicyProvider
{
  Task<DraftPolicySnapshot?> GetDraftPolicyAsync(
    DraftId draftId,
    CancellationToken cancellationToken
  );
}

public sealed record DraftPolicySnapshot(string? FungibleTokenName);
