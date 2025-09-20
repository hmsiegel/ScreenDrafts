namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed partial class DraftPart : Entity<DraftPartId>
{
  private readonly List<DraftRelease> _releases = [];
  private readonly List<TriviaResult> _triviaResults = [];
  private readonly List<Pick> _picks = [];
  private readonly List<DrafterDraftStats> _drafterDraftStats = [];

  private DraftPart(
    Draft draft,
    int partIndex,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeams,
    int totalHosts,
    DraftPartId? id = null)
    : base(id ?? DraftPartId.CreateUnique())
  {
    DraftId = draft.Id;
    Draft = draft;
    PartIndex = partIndex;
    TotalPicks = totalPicks;
    TotalDrafters = totalDrafters;
    TotalDrafterTeams = totalDrafterTeams;
    TotalHosts = totalHosts;
  }

  private DraftPart()
  {
    // For EF
  }

  public DraftId DraftId { get; private set; } = default!;

  public Draft Draft { get; private set; } = default!;

  public int TotalPicks { get; private set; }

  public int PartIndex { get; private set; }

  public DraftStatus DraftStatus { get; private set; } = default!;

  public GameBoard? GameBoard { get; private set; } = default!;

  public DateTime? UpdatedAtUtc { get; private set; }

  public IReadOnlyCollection<DraftRelease> Releases => _releases.AsReadOnly();

  public IReadOnlyCollection<TriviaResult> TriviaResults => _triviaResults.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DrafterStats => _drafterDraftStats.AsReadOnly();


  public static Result<DraftPart> Create(
    Draft draft,
    int partIndex,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeams,
    int totalHosts)
  {
    if (draft == null)
    {
      return Result.Failure<DraftPart>(DraftErrors.DraftIsRequired);
    }

    if (partIndex < 1)
    {
      return Result.Failure<DraftPart>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    var draftPart = new DraftPart(
      draft: draft,
      partIndex: partIndex,
      totalPicks: totalPicks,
      totalDrafters: totalDrafters,
      totalDrafterTeams: totalDrafterTeams,
      totalHosts: totalHosts);

    return Result.Success(draftPart);
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

  public DraftRelease AddRelease(ReleaseChannel channel, DateOnly date, int? episodeNumber = null)
  {
    var release = DraftRelease.Create(
      partId: Id,
      releaseChannel: channel,
      releaseDate: date,
      episodeNumber: episodeNumber).Value;
    _releases.Add(release);
    Raise(new ReleaseAddedDomainEvent(Draft.Id.Value, Id.Value));
    return release;
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

    if (drafter is not null && !Drafters.Contains(drafter))
    {
      return Result.Failure(DrafterErrors.NotFound(drafter.Id.Value));
    }

    if (drafterTeam is not null && !DrafterTeams.Contains(drafterTeam))
    {
      return Result.Failure(DrafterErrors.NotFound(drafterTeamId: drafterTeam.Id.Value));
    }

    var triviaResult = TriviaResult.Create(
      questionsWon: questionsWon,
      position: position,
      draftPart: this,
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


  public void SetGameBoard(GameBoard gameBoard)
  {
    GameBoard = gameBoard;
  }
}
