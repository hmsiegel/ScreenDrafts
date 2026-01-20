namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed partial class DraftPart : Entity<DraftPartId>
{
  private readonly List<DraftRelease> _releases = [];
  private readonly List<TriviaResult> _triviaResults = [];
  private readonly List<Pick> _picks = [];
  private readonly List<DrafterDraftStats> _drafterDraftStats = [];
  private readonly List<ParticipantId> _participants = [];
  private int _communityPicksUsed;

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

  public DraftPartStatus Status { get; private set; } = default!;

  public GameBoard? GameBoard { get; private set; } = default!;

  public DateTime? ScheduledForUtc { get; private set; } = default!;

  public int CommunityPicksUsed => _communityPicksUsed;


  public DateTime? UpdatedAtUtc { get; private set; }

  public IReadOnlyCollection<DraftRelease> Releases => _releases.AsReadOnly();
  public IReadOnlyCollection<TriviaResult> TriviaResults => _triviaResults.AsReadOnly();
  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();
  public IReadOnlyCollection<DrafterDraftStats> DrafterStats => _drafterDraftStats.AsReadOnly();
  public IReadOnlyCollection<ParticipantId> Participants => _participants.AsReadOnly();



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

    if (Status != DraftStatus.InProgress)
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

  internal void IncrementCommunityPicksUsed() => _communityPicksUsed++;
}
