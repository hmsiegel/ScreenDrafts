namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
public sealed partial class DraftPart
{
  private readonly List<Drafter> _drafters = [];
  private readonly List<DrafterTeam> _drafterTeams = [];
  private readonly List<DraftHost> _draftHosts = [];

  public int TotalDrafters { get; private set; }
  public int TotalDrafterTeams { get; private set; }
  public int TotalHosts { get; private set; }

  public int TotalParticipants => _drafters.Count + _drafterTeams.Count;

  public IReadOnlyCollection<Drafter> Drafters => _drafters.AsReadOnly();
  public IReadOnlyCollection<DrafterTeam> DrafterTeams => _drafterTeams.AsReadOnly();
  public IReadOnlyCollection<DraftHost> DraftHosts => _draftHosts.AsReadOnly();
  public DraftHost? PrimaryHost => _draftHosts.FirstOrDefault(h => h.Role == HostRole.Primary);
  public IEnumerable<DraftHost> CoHosts => _draftHosts.Where(h => h.Role == HostRole.CoHost);

  // Drafters
  internal Result AddDrafter(Drafter drafter)
  {
    Guard.Against.Null(drafter);

    if (_drafters.Count >= TotalDrafters)
    {
      return Result.Failure(DraftErrors.TooManyDrafters);
    }

    if (_drafters.Any(d => d.Id == drafter.Id))
    {
      return Result.Failure(DraftErrors.DrafterAlreadyAdded(drafter.Id.Value));
    }

    _drafters.Add(drafter);

    var stats = DrafterDraftStats.Create(drafter: drafter, null, this);

    _drafterDraftStats.Add(stats);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new DrafterAddedDomainEvent(Id.Value, drafter.Id.Value));

    return Result.Success();
  }

  internal Result RemoveDrafter(Drafter drafter)
  {
    Guard.Against.Null(drafter);
    if (!_drafters.Contains(drafter))
    {
      return Result.Failure(DrafterErrors.NotFound(drafter.Id.Value));
    }

    _drafters.Remove(drafter);

    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DrafterRemovedDomainEvent(Id.Value, drafter.Id.Value));
    return Result.Success();
  }

  internal Result SetDrafters(IReadOnlyList<ParticipantId> participants)
  {
    Guard.Against.Null(participants);
    _participants.Clear();
    _participants.AddRange(participants);
    return Result.Success();
  }

  // Drafter Teams
  internal Result AddDrafterTeam(DrafterTeam drafterTeam)
  {
    Guard.Against.Null(drafterTeam);

    if (_drafterTeams.Count >= TotalDrafterTeams)
    {
      return Result.Failure(DraftErrors.TooManyDrafterTeams);
    }

    if (_drafterTeams.Any(d => d.Id == drafterTeam.Id))
    {
      return Result.Failure(DraftErrors.DrafterTeamAlreadyAdded(drafterTeam.Id.Value));
    }

    var existingDrafterIds = _drafterTeams.SelectMany(x => x.Drafters).Select(d => d.Id);

    var overlapping = drafterTeam.Drafters.Where(d => existingDrafterIds.Contains(d.Id)).ToList();

    var overlappingDrafterIds = overlapping.Select(d => d.Id.Value);

    if (overlapping.Count != 0)
    {
      return Result.Failure(DraftErrors.DrafterTeamContainsOverlappingDrafters(overlappingDrafterIds));
    }

    _drafterTeams.Add(drafterTeam);

    var stats = DrafterDraftStats.Create(null, drafterTeam, this);
    _drafterDraftStats.Add(stats);

    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DrafterTeamAddedDomainEvent(Id.Value, drafterTeam.Id.Value));
    return Result.Success();
  }

  internal Result RemoveDrafterTeam(DrafterTeam drafterTeam)
  {
    Guard.Against.Null(drafterTeam);
    if (!_drafterTeams.Contains(drafterTeam))
    {
      return Result.Failure(DrafterErrors.NotFound(drafterTeamId: drafterTeam.Id.Value));
    }
    _drafterTeams.Remove(drafterTeam);
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DrafterTeamRemovedDomainEvent(Id.Value, drafterTeam.Id.Value));
    return Result.Success();
  }

  // Hosts
  internal Result SetPrimaryHost(Host host)
  {
    Guard.Against.Null(host);

    if (PrimaryHost is not null && PrimaryHost.HostId == host.Id)
    {
      return Result.Success();
    }

    if (PrimaryHost is not null)
    {
      return Result.Failure(DraftErrors.PrimaryHostAlreadySet(PrimaryHost.HostId.Value));
    }

    _draftHosts.Add(DraftHost.CreatePrimary(this, host));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new HostAddedDomainEvent(Id.Value, host.Id.Value));

    return Result.Success();
  }

  internal Result AddCoHost(Host host)
  {
    Guard.Against.Null(host);
    if (_draftHosts.Any(h => h.HostId == host.Id && h.Role == HostRole.CoHost))
    {
      return Result.Failure(DraftErrors.HostAlreadyAdded(host.Id.Value));
    }

    if (_draftHosts.Count >= TotalHosts)
    {
      return Result.Failure(DraftErrors.TooManyHosts);
    }

    _draftHosts.Add(DraftHost.CreateCoHost(this, host));
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new HostAddedDomainEvent(Id.Value, host.Id.Value));
    return Result.Success();
  }

  public Result RemoveHost(HostId hostId)
  {
    Guard.Against.Null(hostId);

    var link = _draftHosts.FirstOrDefault(h => h.HostId == hostId);

    if (link is null)
    {
      return Result.Failure(HostErrors.NotFound(hostId.Value));
    }

    _draftHosts.Remove(link);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new HostRemovedDomainEvent(Id.Value, hostId.Value));

    return Result.Success();
  }
}
