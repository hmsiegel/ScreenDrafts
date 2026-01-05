namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Helpers;

/// <summary>
/// Series is the single source of truth for canonical policy and continuity scope.
/// Resolve policies through this provider.
/// </summary>
public interface ISeriesPolicyProvider
{
  ContinuityScope GetContinuityScope(SeriesId seriesId);
  CanonicalPolicy GetCanonicalPolicy(SeriesId seriesId);

  // Compute per-part maxima given the series-level policy
  PartBudget GetPartBudget(SeriesId seriesId, DraftType draftType, int partNumber, int totalParticipants);
}
