namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

internal sealed record ListDraftsResponse
{
  /// <summary>
  /// The draft part's public ID - the primary key for this list row.
  /// </summary>
  public string DraftPartPublicId { get; init; } = default!;

  /// <summary>
  /// The parent draft's public ID.
  /// </summary>
  public string DraftPublicId { get; init; } = default!;

  /// <summary>
  /// "{Title}" for single-part drafts, "{Title} - Part {EpisodeNumber}" for multi-part drafts. Used for display purposes only; the client should not attempt to parse this.
  /// </summary>
  public string Label { get; init; } = default!;

  public int DraftType { get; init; }
  public DraftStatus DraftStatus { get; init; } = default!;
  public DraftPartStatus PartStatus { get; init; } = default!;
  public bool HasCommunityParticipant { get; init; }
  public int TotalPicks { get; init; }

  public IReadOnlyList<ListDraftsReleaseResponse> Releases { get; private set; } = [];
  public IReadOnlyList<ListDraftsParticipantResponse> Participants { get; private set; } = [];
  public IReadOnlyList<ListDraftsHostResponse> Hosts { get; private set; } = [];

  public void SetReleases(IReadOnlyList<ListDraftsReleaseResponse> releases) => Releases = releases;

  public void SetParticipants(IReadOnlyList<ListDraftsParticipantResponse> participants) => Participants = participants;

  public void SetHosts(IReadOnlyList<ListDraftsHostResponse> hosts) => Hosts = hosts;
}
