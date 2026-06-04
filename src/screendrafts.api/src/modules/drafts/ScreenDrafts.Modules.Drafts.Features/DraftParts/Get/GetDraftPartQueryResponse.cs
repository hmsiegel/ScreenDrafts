namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

using GetDraft = Drafts.GetDraft;

internal sealed record GetDraftPartQueryResponse
{
  public string PublicId { get; init; } = default!;
  public int PartIndex { get; init; }
  public int MaxCommunityPicks { get; init; }
  public int MaxCommunityVetoes { get; init; }
  public GetDraft.GetDraftHostResponse? PrimaryHost { get; private set; }
  public Collection<GetDraft.GetDraftHostResponse> CoHosts { get; init; } = [];
  public Collection<GetDraft.GetDraftPartParticipantResponse> Participants { get; init; } = [];
  public Collection<GetDraft.GetDraftReleaseResponse> Releases { get; init; } = [];
  public Collection<GetDraft.GetDraftPickResponse> Picks { get; init; } = [];
  public Collection<GetDraftCommunityFilmRuleResponse> CommunityFilmRules { get; init; } = [];

  public void SetPrimaryHost(GetDraft.GetDraftHostResponse host) => PrimaryHost = host;

  public void AddCoHost(GetDraft.GetDraftHostResponse host) => CoHosts.Add(host);

  public void AddParticipant(GetDraft.GetDraftPartParticipantResponse p) => Participants.Add(p);

  public void AddRelease(GetDraft.GetDraftReleaseResponse r) => Releases.Add(r);

  public void AddPick(GetDraft.GetDraftPickResponse p) => Picks.Add(p);

  public void AddCommunityFilmRule(GetDraftCommunityFilmRuleResponse r) =>
    CommunityFilmRules.Add(r);
}
