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

    DraftPartId? id = null)
    : base(id ?? DraftPartId.CreateUnique())
  {
    DraftId = draftId;
    PartIndex = partIndex;
    MinPosition = minPosition;
    MaxPosition = maxPosition;
    CreatedAtUtc = createdAtUtc;
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
    DraftPartGamePlaySnapshot gameplay)
  {
    if (draftId == null)
    {
      return Result.Failure<DraftPart>(DraftErrors.DraftIsRequired);
    }

    if (partIndex < 1)
    {
      return Result.Failure<DraftPart>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    var draftPart = new DraftPart(
      draftId: draftId,
      partIndex: partIndex,
      minPosition: gameplay.MinPosition,
      maxPosition: gameplay.MaxPosition,
      createdAtUtc: DateTime.UtcNow)
    {
      DraftType = gameplay.DraftType,
      SeriesId = gameplay.SeriesId
    };

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

  internal Result ApplyRollover(ParticipantId participantId, bool isVeto)
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

  internal Result AddTriviaResult(ParticipantId participantId, int position, int questionsWon, bool awardVeto, bool awardVetoOverride)
  {
    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotAddTriviaResultIfDraftIsNotStarted);
    }

    if (!IsParticipantInThisPart(participantId))
    {
      return Result.Failure(DraftPartErrors.ParticipantDoesNotBelongToThisDraftPart(participantId));
    }

    if (_triviaResults.Any(t => t.ParticipantId == participantId))
    {
      return Result.Failure(DraftPartErrors.ParticipantAlreadyHasTriviaResult(participantId));
    }

    var triviaResult = TriviaResult.Create(
      questionsWon: questionsWon,
      position: position,
      draftPart: this,
      participantId: participantId);

    if (triviaResult.IsFailure)
    {
      return Result.Failure(triviaResult.Errors);
    }

    var trivia = triviaResult.Value;

    _triviaResults.Add(trivia);

    var participant = _draftPartParticipants.First(dp => dp.ParticipantId == participantId);

    if (awardVeto)
    {
      participant.AddTriviaAward(isVeto: true);
    }

    if (awardVetoOverride)
    {
      participant.AddTriviaAward(isVeto: false);
    }

    Raise(new TriviaResultAddedDomainEvent(
      Id.Value,
      participantId.Value,
      position,
      questionsWon));

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
}
