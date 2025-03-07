﻿namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

public sealed record DraftResponse(
    Guid Id,
    string Title,
    int DraftType,
    int TotalPicks,
    int TotalDrafters,
    int TotalHosts,
    int EpisodeType,
    int DraftStatus)
{
  private readonly List<DrafterResponse> _drafters = [];
  private readonly List<HostResponse> _hosts = [];
  private readonly List<ReleaseDateResponse>? _releaseDates = [];
  private readonly List<DraftPickResponse>? _draftPicks = [];

  public DraftResponse()
    : this(
        Guid.Empty,
        string.Empty,
        default,
        default,
        default,
        default,
        default,
        default)
  {
  }

  public DateTime[]? RawReleaseDates { get; init; }

  public ReadOnlyCollection<DrafterResponse> Drafters => _drafters.AsReadOnly();
  public ReadOnlyCollection<HostResponse> Hosts => _hosts.AsReadOnly();
  public ReadOnlyCollection<ReleaseDateResponse>? ReleaseDates => _releaseDates!.AsReadOnly();
  public ReadOnlyCollection<DraftPickResponse>? DraftPicks => _draftPicks!.AsReadOnly();

  public void AddDrafter(DrafterResponse drafter) => _drafters.Add(drafter);
  public void AddHost(HostResponse host) => _hosts.Add(host);
  public void AddDraftPick(DraftPickResponse draftPick) => _draftPicks!.Add(draftPick);
  public void AddReleaseDate(ReleaseDateResponse releaseDate) => _releaseDates!.Add(releaseDate);


  public void PopulateReleaseDatesFromRaw()
  {
    if (RawReleaseDates is not null)
    {
      _releaseDates?.Clear();
      _releaseDates?.AddRange(RawReleaseDates!.Select(rd => new ReleaseDateResponse(DateOnly.FromDateTime(rd))));
    }
  }
}
