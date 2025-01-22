namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class Draft : AggrgateRoot<DraftId, Ulid>
{
  private readonly List<Drafter> _drafters = [];
  private readonly List<Pick> _picks = [];
  private readonly List<Host> _hosts = [];
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];
  private readonly List<DrafterDraftStats> _drafterDraftStats = [];
  private readonly List<TriviaResult> _triviaResults = [];

  private Draft(
  DraftId id,
  Title title,
  DraftType draftType,
  int totalPicks,
  int totalDrafters,
  int totalHosts,
  DraftStatus draftStatus,
  DateTime createdAtUtc)
  : base(id)
  {
    Title = title;
    DraftType = draftType;
    TotalPicks = totalPicks;
    TotalDrafters = totalDrafters;
    TotalHosts = totalHosts;
    DraftStatus = draftStatus;
    CreatedAtUtc = createdAtUtc;
  }

  private Draft()
  {
  }

  public int ReadableId { get; init; }

  public Title Title { get; private set; } = default!;

  public DraftType DraftType { get; private set; } = default!;

  public int TotalPicks { get; private set; }

  public int TotalDrafters { get; private set; }

  public int TotalHosts { get; private set; }

  public DraftStatus DraftStatus { get; private set; } = default!;

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime? UpdatedAtUtc { get; private set; }

  public IReadOnlyCollection<Drafter> Drafters => _drafters.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public IReadOnlyCollection<Host> Hosts => _hosts.AsReadOnly();

  public IReadOnlyCollection<Veto> Vetoes => _vetoes.AsReadOnly();

  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DrafterStats => _drafterDraftStats.AsReadOnly();

  public IReadOnlyCollection<TriviaResult> TriviaResults => _triviaResults.AsReadOnly();

  public static Result<Draft> Create(
  Title title,
  DraftType draftType,
  int totalPicks,
  int totalDrafters,
  int totalHosts,
  DraftStatus draftStatus,
  DraftId? id = null)
  {
    if (totalDrafters < 2)
    {
      return Result.Failure<Draft>(DraftErrors.DraftMustHaveAtLeastTwoDrafters);
    }

    var draft = new Draft(
      title: title,
      draftType: draftType,
      totalPicks: totalPicks,
      totalDrafters: totalDrafters,
      totalHosts: totalHosts,
      draftStatus: draftStatus,
      createdAtUtc: DateTime.UtcNow,
      id: id ?? DraftId.CreateUnique());

    draft.Raise(new DraftCreatedDomainEvent(draft.Id.Value));

    return draft;
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

    var stats = DrafterDraftStats.Create(Id.Value, drafter.Id.Value);

    _drafterDraftStats.Add(stats);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new DrafterAddedDomainEvent(Id.Value, drafter.Id.Value));

    return Result.Success();
  }

  public Result AddPick(int position, Movie movie, Drafter drafter)
  {
    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    if (_picks.Any(p => p.Position == position))
    {
      return Result.Failure(DraftErrors.PickPositionAlreadyTaken(position));
    }

    _picks.Add(new Pick(position, movie, drafter));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new PickAddedDomainEvent(
      Id.Value,
      position,
      movie.Id.Value,
      drafter.Id.Value));

    return Result.Success();
  }

  public Result AddHost(Host host)
  {
    Guard.Against.Null(host);
    if (_hosts.Count >= TotalHosts)
    {
      return Result.Failure(DraftErrors.TooManyHosts);
    }

    if (_hosts.Any(h => h.Id == host.Id))
    {
      return Result.Failure(DraftErrors.HostAlreadyAdded(host.Id));
    }

    _hosts.Add(host);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new HostAddedDomainEvent(Id.Value, host.Id));

    return Result.Success();
  }

  public Result AddVeto(Drafter drafter, Pick pick)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotVetoUnlessTheDraftIsStarted);
    }

    if (!_drafters.Contains(drafter))
    {
      return Result.Failure(DraftErrors.OnlyDraftersInTheDraftCanUseAVeto);
    }

    if (!_picks.Contains(pick))
    {
      return Result.Failure(DraftErrors.CannotVetoAPickThatDoesNotExist);
    }

    _vetoes.Add(new Veto(drafter.Id, pick));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new VetoAddedDomainEvent(Id.Value, drafter.Id.Value, pick.Position));

    return Result.Success();
  }

  public Result AddVetoOverride(Drafter drafter, Veto veto)
  {
    ArgumentNullException.ThrowIfNull(drafter);
    ArgumentNullException.ThrowIfNull(veto);

    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotVetoUnlessTheDraftIsStarted);
    }

    if (!_drafters.Contains(drafter))
    {
      return Result.Failure(DraftErrors.OnlyDraftersInTheDraftCanUseAVetoOverride);
    }

    if (!_vetoes.Contains(veto))
    {
      return Result.Failure(DraftErrors.CannotVetoOverrideAVetoThatDoesNotExist);
    }

    _vetoOverrides.Add(new VetoOverride(drafter.Id, veto.Id));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new VetoOverrideAddedDomainEvent(Id.Value, drafter.Id.Value, veto.Id.Value));

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

    if (_hosts.Count != TotalHosts)
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

  public void AddTriviaResult(Ulid drafterId, bool awardIsVeto, int position)
  {
    var triviaResult = new TriviaResult(Id.Value, drafterId, awardIsVeto, position);
    _triviaResults.Add(triviaResult);

    var drafterStats = _drafterDraftStats.FirstOrDefault(d => d.DrafterId == drafterId);
    drafterStats?.AddTriviaAward(awardIsVeto);
  }

  public Result ApplyRollover(Ulid drafterId, bool isVeto)
  {
    var drafterStats = _drafterDraftStats.FirstOrDefault(d => d.DrafterId == drafterId);

    if (isVeto && drafterStats?.RolloversApplied >= 1)
    {
      return Result.Failure(DraftErrors.ADrafterCanOnlyHaveOneRolloverVeto);
    }

    if (!isVeto && drafterStats?.StartingVetoOverrides >=1)
    {
      return Result.Failure(DraftErrors.ADrafterCanOnlyHaveOneRolloverVetoOverride);
    }

    drafterStats?.AddRollover(isVeto);

    return Result.Success();
  }
}
