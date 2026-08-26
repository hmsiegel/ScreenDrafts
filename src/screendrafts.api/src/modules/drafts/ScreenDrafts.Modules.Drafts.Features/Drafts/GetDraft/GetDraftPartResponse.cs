using ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftPartResponse
{
  public string PublicId { get; init; } = default!;
  public int PartIndex { get; init; }
  public DraftType DraftType { get; init; } = default!;
  public DraftPartStatus Status { get; init; } = default!;
  public DateTime? ScheduledForUtc { get; init; }
  public string? PredictionSeasonPublicId { get; init; }

  // Primary nav (ordered by this part's release date, across the whole main feed —
  // scoped by release channel only, not by series. See GetDraftQueryHandler's
  // partAdjacentSql remarks.)
  public string? PreviousDraftPublicId { get; init; }
  public string? PreviousDraftTitle { get; init; }
  public string? NextDraftPublicId { get; init; }
  public string? NextDraftTitle { get; init; }

  // Secondary nav — campaign-scoped (main-feed groupings)
  public string? PreviousCampaignDraftPublicId { get; init; }
  public string? PreviousCampaignDraftTitle { get; init; }
  public string? NextCampaignDraftPublicId { get; init; }
  public string? NextCampaignDraftTitle { get; init; }

  // Secondary nav — series-scoped (e.g. "more Legends Super Drafts"). Only populated
  // when the draft's series has a non-default SeriesKind — see
  // GetDraftQueryHandler.partSeriesAdjacentSql's gating. Works across both main-feed
  // and Patreon channels, unlike campaign nav, which is inherently main-feed-scoped.
  public string? PreviousSeriesDraftPublicId { get; init; }
  public string? PreviousSeriesDraftTitle { get; init; }
  public string? NextSeriesDraftPublicId { get; init; }
  public string? NextSeriesDraftTitle { get; init; }

  public int MaxCommunityPicks { get; init; }
  public int MaxCommunityVetoes { get; init; }
  public Collection<GetDraftCommunityFilmRuleResponse> CommunityFilmRules { get; init; } = [];

  public Collection<GetDraftBoostersChampionAssignmentResponse> BoostersChampionAssignment { get; init; } =
  [];

  public GetDraftHostResponse? PrimaryHost { get; private set; }
  public Collection<GetDraftHostResponse> CoHosts { get; init; } = [];
  public Collection<GetDraftPartParticipantResponse> Participants { get; init; } = [];
  public Collection<GetDraftReleaseResponse> Releases { get; init; } = [];
  public Collection<GetDraftPickResponse> Picks { get; init; } = [];

  /// <summary>
  /// Populated only when DraftType == SpeedDraft. Each entry describes one
  /// sub-draft within the part (index, subject kind, subject name).
  /// </summary>
  public Collection<GetDraftSubDraftResponse> SubDrafts { get; init; } = [];

  public void SetPrimaryHost(GetDraftHostResponse host) => PrimaryHost = host;

  public void AddCoHost(GetDraftHostResponse host) => CoHosts.Add(host);

  public void AddParticipant(GetDraftPartParticipantResponse participant) =>
    Participants.Add(participant);

  public void AddRelease(GetDraftReleaseResponse release) => Releases.Add(release);

  public void AddPick(GetDraftPickResponse pick) => Picks.Add(pick);

  public void AddSubDraft(GetDraftSubDraftResponse subDraft) => SubDrafts.Add(subDraft);

  public void AddCommunityFilmRule(GetDraftCommunityFilmRuleResponse rule) =>
    CommunityFilmRules.Add(rule);

  public void AddBoostersChampionAssignment(
    GetDraftBoostersChampionAssignmentResponse assignment
  ) => BoostersChampionAssignment.Add(assignment);
}
