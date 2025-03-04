﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class Draft : AggrgateRoot<DraftId, Guid>
{
  private readonly List<Drafter> _drafters = [];
  private readonly List<Pick> _picks = [];
  private readonly List<Host> _hosts = [];
  private readonly List<DrafterDraftStats> _drafterDraftStats = [];
  private readonly List<TriviaResult> _triviaResults = [];
  private readonly List<DraftReleaseDate> _releaseDates = [];

  private Draft(
  DraftId id,
  Title title,
  DraftType draftType,
  int totalPicks,
  int totalDrafters,
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

  public int TotalHosts { get; private set; }

  public string? EpisodeNumber { get; private set; } = default!;

  public DraftStatus DraftStatus { get; private set; } = default!;

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime? UpdatedAtUtc { get; private set; }

  public bool IsPatreonOnly { get; private set; }

  // Relationships

  public GameBoard? GameBoard { get; private set; } = default!;

  public IReadOnlyCollection<Drafter> Drafters => _drafters.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public IReadOnlyCollection<Host> Hosts => _hosts.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DrafterStats => _drafterDraftStats.AsReadOnly();

  public IReadOnlyCollection<TriviaResult> TriviaResults => _triviaResults.AsReadOnly();

  public IReadOnlyCollection<DraftReleaseDate> ReleaseDates => _releaseDates.AsReadOnly();


  public static Result<Draft> Create(
  Title title,
  DraftType draftType,
  int totalPicks,
  int totalDrafters,
  int totalHosts,
  DraftStatus draftStatus,
  EpisodeType episodeType,
  DraftId? id = null)
  {
    if (totalDrafters < 2)
    {
      return Result.Failure<Draft>(DraftErrors.DraftMustHaveAtLeastTwoDrafters);
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
      totalHosts: totalHosts,
      draftStatus: draftStatus,
      episodeType: episodeType,
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

    var stats = DrafterDraftStats.Create(drafter, this);

    _drafterDraftStats.Add(stats);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new DrafterAddedDomainEvent(Id.Value, drafter.Id.Value));

    return Result.Success();
  }

  public Result AddPick(int position, Movie movie, Drafter drafter)
  {
    ArgumentNullException.ThrowIfNull(movie);
    ArgumentNullException.ThrowIfNull(drafter);

    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    if (_picks.Any(p => p.Position == position))
    {
      return Result.Failure(DraftErrors.PickPositionAlreadyTaken(position));
    }

    if (position <= 0 || position > TotalPicks)
    {
      return Result.Failure(DraftErrors.PickPositionIsOutOfRange);
    }

    var pick = Pick.Create(position, movie, drafter);

    _picks.Add(pick.Value);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new PickAddedDomainEvent(
      Id.Value,
      position,
      movie.Id,
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
      return Result.Failure(DraftErrors.HostAlreadyAdded(host.Id.Value));
    }

    _hosts.Add(host);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new HostAddedDomainEvent(Id.Value, host.Id.Value));

    return Result.Success();
  }

  public Result AddVeto(Drafter drafter, Pick pick)
  {
    ArgumentNullException.ThrowIfNull(drafter);
    ArgumentNullException.ThrowIfNull(pick);


    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotVetoUnlessTheDraftIsStarted);
    }

    if (!_drafters.Any(d => d.Id.Value == drafter.Id.Value))
    {
      return Result.Failure(DraftErrors.OnlyDraftersInTheDraftCanUseAVeto);
    }

    drafter.AddVeto(pick);

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

    drafter.AddVetoOverride(veto);

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

  public void AddTriviaResult(Drafter drafter, int position, int questionsWon)
  {
    var triviaResult = TriviaResult.Create(
      questionsWon: questionsWon,
      position: position,
      draft: this,
      drafter: drafter);
    _triviaResults.Add(triviaResult.Value);
  }

  public Result ApplyRollover(Guid drafterId, bool isVeto)
  {
    var drafterStats = _drafterDraftStats.FirstOrDefault(d => d.Drafter.Id.Value == drafterId);

    if (isVeto && drafterStats?.RolloversApplied >= 1)
    {
      return Result.Failure(DraftErrors.ADrafterCanOnlyHaveOneRolloverVeto);
    }

    if (!isVeto && drafterStats?.StartingVetoOverrides >= 1)
    {
      return Result.Failure(DraftErrors.ADrafterCanOnlyHaveOneRolloverVetoOverride);
    }

    drafterStats?.AddRollover(isVeto);

    return Result.Success();
  }

  public void SetEpisodeNumber(string episodeNumber)
  {
    Guard.Against.NullOrEmpty(episodeNumber);
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
}
