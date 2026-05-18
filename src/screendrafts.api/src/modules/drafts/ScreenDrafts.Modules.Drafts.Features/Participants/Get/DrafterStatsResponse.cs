namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record DrafterStatsResponse
{
  public required string DrafterPublicId { get; init; }
  public int TotalDrafts { get; init; }
  public DraftBrief? FirstDraft { get; init; }
  public DraftBrief? MostRecentDraft { get; init; }
  public int FilmsDrafted { get; init; }
  public int VetoesUsed { get; init; }
  public int VetoOverridesUsed { get; init; }
  public int CommissionerOverrides { get; init; }
  public int TimesVetoed { get; init; }
  public int TimesVetoOverridden { get; init; }
  public bool HasRolloverVeto { get; init; }
  public bool HasRolloverVetoOverride { get; init; }
}
