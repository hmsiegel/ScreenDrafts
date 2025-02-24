using System.Collections.ObjectModel;

namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

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

  public ReadOnlyCollection<DrafterResponse> Drafters => _drafters.AsReadOnly();
  public ReadOnlyCollection<HostResponse> Hosts => _hosts.AsReadOnly();

  public void AddDrafter(DrafterResponse drafter) => _drafters.Add(drafter);
  public void AddHost(HostResponse host) => _hosts.Add(host);
}
