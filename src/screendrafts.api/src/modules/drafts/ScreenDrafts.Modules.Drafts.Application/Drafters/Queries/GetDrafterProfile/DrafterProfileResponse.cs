
namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafterProfile;

public sealed record DrafterProfileResponse(
  Guid DrafterId,
  Guid PersonId,
  string DisplayName,
  int TotalDrafts,
  DraftBrief? FirstDraft,
  DraftBrief? MostRecentDraft,
  int FilmsDrafted,
  int? VetoesUsed,
  int? VetoOverridesUsed,
  int? CommissionerOverrides,
  int? TimesVetoed,
  int? TimesVetoOverridesAgainst,
  bool HasRolloverVeto,
  bool HasRolloverVetoOverride,
  SocialHandles? SocialHandles,
  IReadOnlyList<DraftHistoryItem> DraftHistory,
  IReadOnlyList<VetoHistoryItem> VetoHistory);
