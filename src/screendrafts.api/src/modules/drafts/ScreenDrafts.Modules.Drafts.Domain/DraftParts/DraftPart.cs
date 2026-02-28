namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart : AggregateRoot<DraftPartId, Guid>
{
  private readonly List<DraftRelease> _releases = [];
  private readonly List<TriviaResult> _triviaResults = [];
  private readonly List<Pick> _picks = [];
  private readonly List<RequiredMovieVersion> _requiredMovieVersions = [];
  private int _communityPicksUsed;

  private DraftPart(
    DraftId draftId,
    int partIndex,
    int? minPosition,
    int? maxPosition,
    DateTime createdAtUtc,
    string publicId,
    DraftPartId? id = null)
    : base(id ?? DraftPartId.CreateUnique())
  {
    DraftId = draftId;
    PartIndex = partIndex;
    MinPosition = minPosition;
    MaxPosition = maxPosition;
    CreatedAtUtc = createdAtUtc;
    PublicId = publicId;
    Status = DraftPartStatus.Created;
    MovieVersionPolicyType = DraftPartMovieVersionPolicyType.None;
  }

  private DraftPart()
  {
    // For EF
  }

  public DraftId DraftId { get; private set; } = default!;
  public int? TotalPicks
  {
    get
    {
      if (!MinPosition.HasValue && !MaxPosition.HasValue)
      {
        return null;
      }

      var min = MinPosition!.Value;
      var max = MaxPosition!.Value;

      return (MinPosition.Value > 0 && MaxPosition.Value >= MinPosition.Value)
        ? (max - min + 1)
        : null;
    }
  }
  public string PublicId { get; private set; } = default!;
  public int PartIndex { get; private set; }
  public DraftPartStatus Status { get; private set; } = default!;
  public GameBoard? GameBoard { get; private set; } = default!;
  public DateTime? ScheduledForUtc { get; private set; } = default!;
  public SeriesId SeriesId { get; private set; } = default!;
  public DraftType DraftType { get; private set; } = default!;

  public int CommunityPicksUsed => _communityPicksUsed;
  public int? MinPosition { get; private set; }
  public int? MaxPosition { get; private set; }

  public DateTime CreatedAtUtc { get; private set; }
  public DateTime? UpdatedAtUtc { get; private set; }

  public DraftPartMovieVersionPolicyType MovieVersionPolicyType { get; private set; } = default!;

  public IReadOnlyCollection<DraftRelease> Releases => _releases.AsReadOnly();
  public IReadOnlyCollection<TriviaResult> TriviaResults => _triviaResults.AsReadOnly();
  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();
  public IReadOnlyCollection<RequiredMovieVersion> RequiredMovieVersions => _requiredMovieVersions.AsReadOnly();

  public static Result<DraftPart> Create(
    DraftId draftId,
    int partIndex,
    DraftPartGamePlaySnapshot gameplay,
    string publicId)
  {
    if (draftId == null)
    {
      return Result.Failure<DraftPart>(DraftErrors.DraftIsRequired);
    }

    if (partIndex < 1)
    {
      return Result.Failure<DraftPart>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DraftPart>(DraftPartErrors.InvalidPublicId);
    }

    var draftPart = new DraftPart(
      draftId: draftId,
      partIndex: partIndex,
      minPosition: gameplay.MinPosition,
      maxPosition: gameplay.MaxPosition,
      createdAtUtc: DateTime.UtcNow,
      publicId: publicId)
    {
      DraftType = gameplay.DraftType,
      SeriesId = gameplay.SeriesId
    };

    var gameBoardResult = GameBoard.Create(draftPart);

    if (gameBoardResult.IsFailure)
    {
      return Result.Failure<DraftPart>(gameBoardResult.Errors);
    }

    draftPart.GameBoard = gameBoardResult.Value;

    return Result.Success(draftPart);
  }

  internal static Result<DraftPart> SeedCreate(
    DraftId draftId,
    int partIndex,
    int minPosition,
    int maxPosition,
    DraftType draftType,
    SeriesId seriesId,
    DraftPartStatus status,
    string publicId,
    DateTime? scheduledForUtc = null,
    DraftPartId? id = null,
    DateTime? createdAtUtc = null)
  {
    if (draftId == null)
    {
      return Result.Failure<DraftPart>(DraftErrors.DraftIsRequired);
    }

    if (partIndex < 1)
    {
      return Result.Failure<DraftPart>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    if (minPosition <= 0)
    {
      return Result.Failure<DraftPart>(DraftPartErrors.MinimumPositionMustBeGreaterThanZero);
    }

    if (maxPosition <= 0)
    {
      return Result.Failure<DraftPart>(DraftPartErrors.MaxPositionIsOutOfRange);
    }

    if (maxPosition < minPosition)
    {
      return Result.Failure<DraftPart>(DraftPartErrors.MaxPositionIsOutOfRange);
    }

    var draftPart = new DraftPart(
      draftId: draftId,
      partIndex: partIndex,
      minPosition: minPosition,
      maxPosition: maxPosition,
      publicId: publicId,
      createdAtUtc: createdAtUtc ?? DateTime.UtcNow,
      id: id)
    {
      Status = status,
      SeriesId = seriesId,
      DraftType = draftType,
      ScheduledForUtc = scheduledForUtc
    };
    return draftPart;
  }

  internal Result ApplyRollover(Participant participantId, bool isVeto)
  {
    if (!IsParticipantInThisPart(participantId))
    {
      return Result.Failure(DraftPartErrors.ParticipantDoesNotBelongToThisDraftPart(participantId));
    }

    var participant = GetParticipantRequired(participantId);

    participant.AddRollover(isVeto);

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }


  public DraftRelease AddRelease(ReleaseChannel channel, DateOnly date)
  {
    var release = DraftRelease.Create(
      partId: Id,
      releaseChannel: channel,
      releaseDate: date).Value;
    _releases.Add(release);
    Raise(new ReleaseAddedDomainEvent(DraftId.Value, Id.Value));
    return release;
  }

  public Result AssignTriviaResults(IEnumerable<(Participant participant, int Position, int QuestionsWon)> results)
  {
    ArgumentNullException.ThrowIfNull(results);

    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftPartErrors.InvalidStatusForTriviaAssignment);
    }

    _triviaResults.Clear();

    foreach (var (participant, position, questionsWon) in results)
    {
      if (_triviaResults.Any(t => t.ParticipantId == participant))
      {
        return Result.Failure(DraftPartErrors.ParticipantAlreadyHasTriviaResult(participant));
      }

      var result = TriviaResult.Create(
        participantId: participant,
        position: position,
        questionsWon: questionsWon,
        draftPart: this);

      if (result.IsFailure)
      {
        return Result.Failure(result.Errors);
      }

      _triviaResults.Add(result.Value);
    }

    Raise(new TriviaResultsAssignedDomainEvent(
      draftPartId: Id.Value,
      triviaResults: _triviaResults
        .Select(t => (t.ParticipantId.Value, t.Position))
        .ToList()
        .AsReadOnly()));

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }



  public void SetGameBoard(GameBoard gameBoard)
  {
    GameBoard = gameBoard;
  }

  public Result SetPartPositions(int minPosition, int maxPosition)
  {
    if (minPosition <= 0)
    {
      return Result.Failure(DraftPartErrors.MinimumPositionMustBeGreaterThanZero);
    }

    if (maxPosition <= 0)
    {
      return Result.Failure(DraftPartErrors.MaxPositionIsOutOfRange);
    }

    if (maxPosition < minPosition)
    {
      return Result.Failure(DraftPartErrors.MaxPositionIsOutOfRange);
    }

    MinPosition = minPosition;
    MaxPosition = maxPosition;

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  internal void IncrementCommunityPicksUsed() => _communityPicksUsed++;

  internal Result SetPartIndex(int partIndex)
  {
    if (partIndex <= 0)
    {
      return Result.Failure(DraftPartErrors.PartIndexIsOutOfRange);
    }

    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(DraftPartErrors.CannotChangePartIndexAfterDraftHasStarted);
    }

    PartIndex = partIndex;
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  internal Result UpdatePublicId(string publicId)
  {
    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure(DraftPartErrors.InvalidPublicId);
    }

    PublicId = publicId;

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }
}
