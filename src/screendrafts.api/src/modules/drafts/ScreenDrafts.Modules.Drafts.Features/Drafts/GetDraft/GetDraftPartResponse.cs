namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftPartResponse
{
  public string PublicId { get; init; } = default!;
  public int PartIndex { get; init; }
  public int DraftType { get; init; }
  public int Status { get; init; }
  public DateTime? ScheduledForUtc { get; init; }

  public GetDraftHostResponse? PrimaryHost { get; private set; }
  public Collection<GetDraftHostResponse> CoHosts { get; init; } = [];
  public Collection<GetDraftPartParticipantResponse> Participants { get; init; } = [];
  public Collection<GetDraftReleaseResponse> Releases { get; init; } = [];
  public Collection<GetDraftPickResponse> Picks { get; init; } = [];

  public void SetPrimaryHost(GetDraftHostResponse host) => PrimaryHost = host;
  public void AddCoHost(GetDraftHostResponse host) => CoHosts.Add(host);
  public void AddParticipant(GetDraftPartParticipantResponse participant) => Participants.Add(participant);
  public void AddRelease(GetDraftReleaseResponse release) => Releases.Add(release);
  public void AddPick(GetDraftPickResponse pick) => Picks.Add(pick);
}
