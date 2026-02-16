namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;
public sealed partial class DraftPart
{
  private readonly List<DraftHost> _draftHosts = [];
  private readonly List<DraftPartParticipant> _draftPartParticipants = [];

  public int TotalDrafters => Participants.Count(p => p.Kind == ParticipantKind.Drafter);
  public int TotalDrafterTeams => Participants.Count(p => p.Kind == ParticipantKind.Team);
  public int TotalHosts => DraftHosts.Count;
  public int TotalParticipants => Participants.Count;

  public IReadOnlyCollection<DraftHost> DraftHosts => _draftHosts;
  public DraftHost? PrimaryHost => _draftHosts.FirstOrDefault(h => h.Role == HostRole.Primary);
  public IEnumerable<DraftHost> CoHosts => _draftHosts.Where(h => h.Role == HostRole.CoHost);
  public IReadOnlyCollection<ParticipantId> Participants => _draftPartParticipants
    .Select(dp => dp.ParticipantId)
    .ToList()
    .AsReadOnly();

  // Participants
  public Result SetParticipants(IReadOnlyList<ParticipantId> participants)
  {
    Guard.Against.Null(participants);
    _draftPartParticipants.Clear();
    _draftPartParticipants.AddRange(
      participants.Select(p => DraftPartParticipant.Create(this, p))
    );
    return Result.Success();
  }

  public Result AddParticipant(ParticipantId participant)
  {
    Guard.Against.Null(participant);
    if (_draftPartParticipants.Any(dp => dp.ParticipantId == participant))
    {
      return Result.Failure(DraftPartErrors.ParticipantAlreadyAdded(participant.Value));
    }
    _draftPartParticipants.Add(DraftPartParticipant.Create(this, participant));
    return Result.Success();
  }

  public Result RemoveParticipant(ParticipantId participant)
  {
    Guard.Against.Null(participant);
    var link = _draftPartParticipants.FirstOrDefault(dp => dp.ParticipantId == participant);
    if (link is null)
    {
      return Result.Failure(DraftPartErrors.ParticipantNotFound(participant.Value));
    }
    _draftPartParticipants.Remove(link);
    return Result.Success();
  }

  public bool HasParticipant(ParticipantId participant)
  {
    Guard.Against.Null(participant);
    return _draftPartParticipants.Any(dp => dp.ParticipantId == participant);
  }
  private DraftPartParticipant GetParticipantRequired(ParticipantId participantId)
  {
    var participant = _draftPartParticipants.FirstOrDefault(dp => dp.ParticipantId == participantId);
    return participant is null
      ? throw new ArgumentException($"Participant not found: {participantId}", nameof(participantId))
      : participant;
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

  public Result SetHosts(IReadOnlyList<DraftHost> hosts)
  {
    _draftHosts.Clear();
    _draftHosts.AddRange(hosts);
    return Result.Success();
  }
}
