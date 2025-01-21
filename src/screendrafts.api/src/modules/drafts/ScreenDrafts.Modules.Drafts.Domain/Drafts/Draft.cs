namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class Draft : AggrgateRoot<DraftId, Ulid>
{
  private readonly List<Drafter> _drafters = [];
  private readonly List<Pick> _picks = [];
  private readonly List<Host> _hosts = [];
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];

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

  public static Draft Create(
  Title title,
  DraftType draftType,
  int totalPicks,
  int totalDrafters,
  int totalHosts,
  DraftStatus draftStatus,
  DraftId? id = null)
  {
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
      return Result.Fail(DraftErrors.TooManyDrafters);
    }

    if (_drafters.Any(d => d.Id == drafter.Id))
    {
      return Result.Fail(DraftErrors.DrafterAlreadyAddes(drafter.Id.Value));
    }

    _drafters.Add(drafter);
    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new DrafterAddedDomainEvent(Id.Value, drafter.Id.Value));

    return Result.Ok();
  }

  public Result AddPick(int position, Movie movie, Drafter drafter)
  {
    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Fail(DraftErrors.DraftNotStarted);
    }

    if (_picks.Any(p => p.Position == position))
    {
      return Result.Fail(DraftErrors.PickPositionAlreadyTaken(position));
    }

    _picks.Add(new Pick(position, movie, drafter));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new PickAddedDomainEvent(
      Id.Value,
      position,
      movie.Id.Value,
      drafter.Id.Value));

    return Result.Ok();
  }

  public Result AddHost(Host host)
  {
    Guard.Against.Null(host);
    if (_hosts.Count >= TotalHosts)
    {
      return Result.Fail(DraftErrors.TooManyHosts);
    }

    if (_hosts.Any(h => h.Id == host.Id))
    {
      return Result.Fail(DraftErrors.HostAlreadyAdded(host.Id));
    }

    _hosts.Add(host);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new HostAddedDomainEvent(Id.Value, host.Id));

    return Result.Ok();
  }

  public Result AddVeto(Drafter drafter, Pick pick)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Fail(DraftErrors.CannotVetoUnlessTheDraftIsStarted);
    }

    if (!_drafters.Contains(drafter))
    {
      return Result.Fail(DraftErrors.OnlyDraftersInTheDraftCanUseAVeto);
    }

    if (!_picks.Contains(pick))
    {
      return Result.Fail(DraftErrors.CannotVetoAPickThatDoesNotExist);
    }

    _vetoes.Add(new Veto(drafter.Id, pick));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new VetoAddedDomainEvent(Id.Value, drafter.Id.Value, pick.Position));

    return Result.Ok();
  }

  public Result AddVetoOverride(Drafter drafter, Veto veto)
  {
    ArgumentNullException.ThrowIfNull(drafter);
    ArgumentNullException.ThrowIfNull(veto);

    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Fail(DraftErrors.CannotVetoUnlessTheDraftIsStarted);
    }

    if (!_drafters.Contains(drafter))
    {
      return Result.Fail(DraftErrors.OnlyDraftersInTheDraftCanUseAVetoOverride);
    }

    if (!_vetoes.Contains(veto))
    {
      return Result.Fail(DraftErrors.CannotVetoOverrideAVetoThatDoesNotExist);
    }

    _vetoOverrides.Add(new VetoOverride(drafter.Id, veto.Id));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new VetoOverrideAddedDomainEvent(Id.Value, drafter.Id.Value, veto.Id.Value));

    return Result.Ok();
  }

  public Result StartDraft()
  {
    if (DraftStatus != DraftStatus.Created)
    {
      return Result.Fail(DraftErrors.DraftCanOnlyBeStartedIfItIsCreated);
    }

    if (_drafters.Count != TotalDrafters)
    {
      return Result.Fail(DraftErrors.CannotStartDraftWithoutAllDrafters);
    }

    if (_hosts.Count != TotalHosts)
    {
      return Result.Fail(DraftErrors.CannotStartDraftWithoutAllHosts);
    }

    DraftStatus = DraftStatus.InProgress;

    Raise(new DraftStartedDomainEvent(Id.Value));

    return Result.Ok();
  }

  public Result CompleteDraft()
  {
    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Fail(DraftErrors.CannotCompleteDraftIfItIsNotInProgress);
    }

    if (_picks.Count != TotalPicks)
    {
      return Result.Fail(DraftErrors.CannotCompleteDraftWithoutAllPicks);
    }

    DraftStatus = DraftStatus.Completed;

    Raise(new DraftCompletedDomainEvent(Id.Value));

    return Result.Ok();
  }
}
