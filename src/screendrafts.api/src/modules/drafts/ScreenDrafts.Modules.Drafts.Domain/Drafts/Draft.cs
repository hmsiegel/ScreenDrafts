namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class Draft : AggrgateRoot<DraftId, Guid>
{
  private readonly List<Drafter> _drafters = [];
  private readonly List<DrafterTeam> _drafterTeams = [];
  private readonly List<Pick> _picks = [];
  private readonly List<DraftHost> _draftHosts = [];
  private readonly List<DrafterDraftStats> _drafterDraftStats = [];
  private readonly List<TriviaResult> _triviaResults = [];
  private readonly List<DraftReleaseDate> _releaseDates = [];

  private Draft(
  DraftId id,
  Title title,
  DraftType draftType,
  int totalPicks,
  int totalDrafters,
  int totalDrafterTeams,
  int totalHosts,
  DraftStatus draftStatus,
  EpisodeType episodeType,
  DateTime createdAtUtc)
  : base(id)
  {
    Title = title;
    DraftType = draftType;
    TotalPicks = totalPicks;
    TotalDrafters = totalDrafters;
    TotalDrafterTeams = totalDrafterTeams;
    TotalHosts = totalHosts;
    DraftStatus = draftStatus;
    EpisodeType = episodeType;
    CreatedAtUtc = createdAtUtc;
  }

  private Draft()
  {
  }

  public int ReadableId { get; init; }

  public Title Title { get; private set; } = default!;

  public DraftType DraftType { get; private set; } = default!;

  public EpisodeType EpisodeType { get; private set; } = default!;

  public int TotalPicks { get; private set; }

  public int TotalDrafters { get; private set; }

  public int TotalDrafterTeams { get; private set; }

  public int TotalHosts { get; private set; }

  public int? EpisodeNumber { get; private set; } = default!;

  public DraftStatus DraftStatus { get; private set; } = default!;

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime? UpdatedAtUtc { get; private set; }

  public bool IsPatreonOnly { get; private set; }

  public bool NonCanonical { get; private set; }

  public bool IsScreamDrafts { get; private set; }

  public string? Description { get; private set; }

  // Relationships

  public GameBoard? GameBoard { get; private set; } = default!;

  public IReadOnlyCollection<Drafter> Drafters => _drafters.AsReadOnly();

  public IReadOnlyCollection<DrafterTeam> DrafterTeams => _drafterTeams.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public IReadOnlyCollection<DraftHost> DraftHosts => _draftHosts.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DrafterStats => _drafterDraftStats.AsReadOnly();

  public IReadOnlyCollection<TriviaResult> TriviaResults => _triviaResults.AsReadOnly();

  public IReadOnlyCollection<DraftReleaseDate> ReleaseDates => _releaseDates.AsReadOnly();

  public DraftHost? PrimaryHost => _draftHosts.FirstOrDefault(h => h.Role == HostRole.Primary);

  public IEnumerable<DraftHost> CoHosts => _draftHosts.Where(h => h.Role == HostRole.CoHost);


  public static Result<Draft> Create(
  Title title,
  DraftType draftType,
  int totalPicks,
  int totalDrafters,
  int totalDrafterTeams,
  int totalHosts,
  DraftStatus draftStatus,
  EpisodeType episodeType,
  DraftId? id = null)
  {
    if (totalDrafters + totalDrafterTeams < 2)
    {
      return Result.Failure<Draft>(DraftErrors.DraftMustHaveAtLeastTwoParticipants);
    }

    if (totalPicks < 4)
    {
      return Result.Failure<Draft>(DraftErrors.DraftMustHaveAtLeastFivePicks);
    }

    var draft = new Draft(
      title: title,
      draftType: draftType,
      totalPicks: totalPicks,
      totalDrafters: totalDrafters,
      totalDrafterTeams: totalDrafterTeams,
      totalHosts: totalHosts,
      draftStatus: draftStatus,
      episodeType: episodeType,
      createdAtUtc: DateTime.UtcNow,
      id: id ?? DraftId.CreateUnique());

    draft.Raise(new DraftCreatedDomainEvent(draft.Id.Value));

    return draft;
  }

  public Result EditDraft(
    Title title,
    DraftType draftType,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeams,
    int totalHosts,
    EpisodeType episodeType,
    DraftStatus draftStatus,
    string? description)
  {
    Guard.Against.Null(title);
    Guard.Against.Null(draftType);
    Guard.Against.Null(episodeType);
    Guard.Against.NullOrWhiteSpace(description);

    if (totalDrafters + totalDrafterTeams < 2)
    {
      return Result.Failure(DraftErrors.DraftMustHaveAtLeastTwoParticipants);
    }

    if (totalPicks < 4)
    {
      return Result.Failure(DraftErrors.DraftMustHaveAtLeastFivePicks);
    }

    if (draftStatus != DraftStatus.Created)
    {
      return Result.Failure(DraftErrors.CannotEditADraftAfterItHasBeenStarted);
    }

    Title = title;
    DraftType = draftType;
    TotalPicks = totalPicks;
    TotalDrafters = totalDrafters;
    TotalDrafterTeams = totalDrafterTeams;
    TotalHosts = totalHosts;
    EpisodeType = episodeType;
    DraftStatus = draftStatus;
    UpdatedAtUtc = DateTime.UtcNow;
    Description = description;

    Raise(new DraftEditedDomainEvent(Id.Value, title.Value));

    return Result.Success();
  }

  public Result AddDrafter(Drafter drafter)
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

  public Result AddDrafterTeam(DrafterTeam drafterTeam)
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

  public Result AddPick(Pick pick)
  {
    Guard.Against.Null(pick);

    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    if (_picks.Any(p => p.Position == pick.Position))
    {
      return Result.Failure(DraftErrors.PickPositionAlreadyTaken(pick.Position));
    }

    if (pick.Position <= 0 || pick.Position > TotalPicks)
    {
      return Result.Failure(DraftErrors.PickPositionIsOutOfRange);
    }

    _picks.Add(pick);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new PickAddedDomainEvent(
      Id.Value,
      pick.Position,
      pick.Movie.Id,
      pick.Drafter!.Id.Value,
      null));

    return Result.Success();
  }

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

    if (_draftHosts.Count >= TotalHosts)
    {
      return Result.Failure(DraftErrors.TooManyHosts);
    }

    _draftHosts.Add(DraftHost.CreateCoHost(this, host));
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new HostAddedDomainEvent(Id.Value, host.Id.Value));
    return Result.Success();
  }

  public Result StartDraft()
  {
    if (DraftStatus != DraftStatus.Created)
    {
      return Result.Failure(DraftErrors.DraftCanOnlyBeStartedIfItIsCreated);
    }

    if (_drafters.Count != TotalDrafters)
    {
      return Result.Failure(DraftErrors.CannotStartDraftWithoutAllDrafters);
    }

    if (PrimaryHost is null)
    {
      return Result.Failure(DraftErrors.CannotStartDraftWithoutAllHosts);
    }

    if (_draftHosts.Count != TotalHosts)
    {
      return Result.Failure(DraftErrors.CannotStartDraftWithoutAllHosts);
    }

    DraftStatus = DraftStatus.InProgress;

    Raise(new DraftStartedDomainEvent(Id.Value));

    return Result.Success();
  }


  public Result CompleteDraft()
  {
    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotCompleteDraftIfItIsNotInProgress);
    }

    if (_picks.Count != TotalPicks)
    {
      return Result.Failure(DraftErrors.CannotCompleteDraftWithoutAllPicks);
    }

    DraftStatus = DraftStatus.Completed;

    Raise(new DraftCompletedDomainEvent(Id.Value));

    return Result.Success();
  }

  public Result AddTriviaResult(Drafter? drafter, DrafterTeam? drafterTeam, int position, int questionsWon)
  {
    if (drafter is null && drafterTeam is null)
    {
      return Result.Failure(DraftErrors.CannotAddTriviaResultWithoutDrafterOrDrafterTeam);
    }

    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotAddTriviaResultIfDraftIsNotStarted);
    }

    if (drafter is not null && !_drafters.Contains(drafter))
    {
      return Result.Failure(DrafterErrors.NotFound(drafter.Id.Value));
    }

    if (drafterTeam is not null && !_drafterTeams.Contains(drafterTeam))
    {
      return Result.Failure(DrafterErrors.NotFound(drafterTeamId: drafterTeam.Id.Value));
    }

    var triviaResult = TriviaResult.Create(
      questionsWon: questionsWon,
      position: position,
      draft: this,
      drafter: drafter,
      drafterTeam: drafterTeam).Value;

    _triviaResults.Add(triviaResult);

    Raise(new TriviaResultAddedDomainEvent(
      Id.Value,
      drafter?.Id.Value,
      position,
      questionsWon,
      drafterTeam?.Id.Value));

    return Result.Success();
  }

  public Result ApplyRollover(Guid? drafterId, Guid? drafterTeamId, bool isVeto)
  {
    var drafterStats =
       _drafterDraftStats.FirstOrDefault(d => d.Drafter?.Id.Value == drafterId)
       ?? _drafterDraftStats.FirstOrDefault(d => d.DrafterTeam?.Id.Value == drafterTeamId);

    if (isVeto && drafterStats?.RolloverVeto >= 1)
    {
      return Result.Failure(DraftErrors.ADrafterCanOnlyHaveOneRolloverVeto);
    }

    if (!isVeto && drafterStats?.RolloverVetoOverride >= 1)
    {
      return Result.Failure(DraftErrors.ADrafterCanOnlyHaveOneRolloverVetoOverride);
    }

    drafterStats?.AddRollover(isVeto);

    return Result.Success();
  }

  public Result ApplyCommissionerOverride(Pick pick)
  {
    ArgumentNullException.ThrowIfNull(pick);

    var overrideEntry = CommissionerOverride.Create(pick).Value;

    pick.ApplyCommissionerOverride(overrideEntry);

    var drafterStats = _drafterDraftStats
      .FirstOrDefault(d => d.Drafter?.Id.Value == pick.DrafterId?.Value);

    drafterStats?.AddCommissionerOverride();

    return Result.Success();
  }

  public void SetEpisodeNumber(int episodeNumber)
  {
    Guard.Against.Zero(episodeNumber);
    EpisodeNumber = episodeNumber;
  }

  public Result PauseDraft()
  {
    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotPauseDraftIfItIsNotInProgress);
    }
    DraftStatus = DraftStatus.Paused;
    Raise(new DraftPausedDomainEvent(Id.Value));
    return Result.Success();
  }

  public Result ContinueDraft()
  {
    if (DraftStatus != DraftStatus.Paused)
    {
      return Result.Failure(DraftErrors.CannotContinueDraftIfItIsNotPaused);
    }
    DraftStatus = DraftStatus.InProgress;
    Raise(new DraftContinuedDomainEvent(Id.Value));
    return Result.Success();
  }

  public void AddReleaseDate(DraftReleaseDate releaseDate)
  {
    _releaseDates.Add(releaseDate);
  }

  public void SetDraftStatus(DraftStatus draftStatus)
  {
    DraftStatus = draftStatus;
  }

  public void SetGameBoard(GameBoard gameBoard)
  {
    GameBoard = gameBoard;
  }

  public void SetPatreonOnly(bool isPatreonOnly)
  {
    IsPatreonOnly = isPatreonOnly;
  }

  public void SetNonCanonical(bool nonCanonical)
  {
    NonCanonical = nonCanonical;
  }

  public void SetScreamDrafts(bool screamDrafts)
  {
    IsScreamDrafts = screamDrafts;
  }

  public Result RemoveDrafter(Drafter drafter)
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

  public Result RemoveHost(Host host)
  {
    Guard.Against.Null(host);

    var link = _draftHosts.FirstOrDefault(h => h.HostId == host.Id);

    if (link is null)
    {
      return Result.Failure(HostErrors.NotFound(host.Id.Value));
    }

    _draftHosts.Remove(link);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new HostRemovedDomainEvent(Id.Value, host.Id.Value));

    return Result.Success();
  }

  public Result RemoveDrafterTeam(DrafterTeam drafterTeam)
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
}
