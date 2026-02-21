namespace ScreenDrafts.Modules.Drafts.Infrastructure.SeriesInfrastructure;

internal sealed class NullSeriesPolicyProvider : ISeriesPolicyProvider
{
  public CanonicalPolicy GetCanonicalPolicy(SeriesId seriesId)
    => CanonicalPolicy.Always;

  public ContinuityScope GetContinuityScope(SeriesId seriesId)
    => ContinuityScope.Global;

  public PartBudget GetPartBudget(SeriesId seriesId, DraftType draftType, int partNumber, int totalParticipants)
    => new(MaxVetoes: 0, MaxVetoOverrides: 0);
}
