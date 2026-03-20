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
  public IReadOnlyCollection<Participant> Participants => _draftPartParticipants
    .Select(dp => dp.ParticipantId)
    .ToList()
    .AsReadOnly();

  // Participants
  public Result SetParticipants(IReadOnlyList<Participant> participants)
  {
    Guard.Against.Null(participants);
    _draftPartParticipants.Clear();
    _draftPartParticipants.AddRange(
      participants.Select(p => DraftPartParticipant.Create(this, p))
    );
    return Result.Success();
  }

  public Result AddParticipant(Participant participant)
  {
    Guard.Against.Null(participant);
    if (_draftPartParticipants.Any(dp => dp.ParticipantId == participant))
    {
      return Result.Failure(DraftPartErrors.ParticipantAlreadyAdded(participant.Value));
    }
    _draftPartParticipants.Add(DraftPartParticipant.Create(this, participant));
    return Result.Success();
  }

  public Result SetParticipantAward(Participant participant, bool isVeto)
  {
    var p = GetParticipantRequired(participant);
    p.GrantAward(isVeto);

    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result RevokeParticipantAward(Participant participant, bool isVeto)
  {
    var p = GetParticipantRequired(participant);
    p.RevokeAward(isVeto);
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result RemoveParticipant(Participant participant)
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

  public bool HasParticipant(Participant participant)
  {
    Guard.Against.Null(participant);
    return _draftPartParticipants.Any(dp => dp.ParticipantId == participant);
  }

  public Result InitializeParticipantVetoes(
    Participant participant,
    int startingVetoes,
    int vetoesRollingIn,
    int vetoOverridesRollingIn)
  {
    if (!IsParticipantInThisPart(participant))
    {
      return Result.Failure(DraftPartErrors.ParticipantDoesNotBelongToThisDraftPart(participant));
    }

    var p = GetParticipantRequired(participant);
    p.InitializeVetoes(startingVetoes, vetoesRollingIn, vetoOverridesRollingIn);

    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  internal DraftPartParticipant GetParticipantRequired(Participant participantId)
  {
    var participant = _draftPartParticipants.FirstOrDefault(dp => dp.ParticipantIdValue == participantId.Value);
    return participant is null
      ? throw new ArgumentException($"Participant not found: {participantId}", nameof(participantId))
      : participant;
  }

  // Hosts
  public Result SetPrimaryHost(Host host)
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

  public Result AddCoHost(Host host)
  {
    Guard.Against.Null(host);
    if (_draftHosts.Any(h => h.HostId == host.Id && h.Role == HostRole.CoHost))
    {
      return Result.Failure(DraftErrors.HostAlreadyAdded(host.Id.Value));
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
