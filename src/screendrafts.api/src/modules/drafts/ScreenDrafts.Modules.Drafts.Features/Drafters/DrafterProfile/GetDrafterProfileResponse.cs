namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

internal sealed record GetDrafterProfileResponse
{
  public required string DrafterPublicId { get; init; }
  public required string PersonPublicId { get; init; }
  public required string DisplayName { get; init; }
  public int TotalDrafts { get; init; }
  public DraftBrief? FirstDraft { get; init; }
  public DraftBrief? MostRecentDraft { get; init; }
  public int FilmsDrafted { get; init; }
  public int VetoesUsed { get; init; }
  public int VetoOverridesUsed { get; init; }
  public int CommissionerOverrides { get; init; }
  public int TimesVetoed { get; init; }
  public int TimeesVetoOverridden { get; init; }
  public bool HasRolloverVeto { get; init; }
  public bool HasRolloverVetoOverride { get; init; }
  public SocialHandles? SocialHandles { get; init; }
  public IReadOnlyList<DraftHistoryItem> DraftHistory { get; init; } = [];
  public IReadOnlyList<VetoHistoryItem> VetoHistory { get; init; } = [];
}
