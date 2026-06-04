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

  // Per-part adjacent navigation (ordered by this part's release date within the series)
  public string? PreviousDraftPublicId { get; init; }
  public string? PreviousDraftTitle { get; init; }
  public string? NextDraftPublicId { get; init; }
  public string? NextDraftTitle { get; init; }
  public string? PreviousCampaignDraftPublicId { get; init; }
  public string? PreviousCampaignDraftTitle { get; init; }
  public string? NextCampaignDraftPublicId { get; init; }
  public string? NextCampaignDraftTitle { get; init; }

  public int MaxCommunityPicks { get; init; }
  public int MaxCommunityVetoes { get; init; }
  public Collection<GetDraftCommunityFilmRuleResponse> CommunityFilmRules { get; init; } = [];

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
}
