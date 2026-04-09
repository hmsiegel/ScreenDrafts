namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class DraftPredictionSet : Entity<DraftPredictionSetId>
{
  private readonly List<PredictionEntry> _entries = [];
  private readonly List<SurrogateAssignment> _surrogates = [];

  private DraftPredictionSet(
    string publicId,
    PredictionSeason season,
    DraftPart draftPart,
    PredictionContestant contestant,
    Person? submittedByPerson,
    PredictionSourceKind sourceKind,
    DraftPredictionSetId? id = null)
    : base(id ?? DraftPredictionSetId.CreateUnique())
  {
    PublicId = publicId;

    Season = season;
    SeasonId = season.Id;

    DraftPart = draftPart;
    DraftPartId = draftPart.Id;

    Contestant = contestant;
    ContestantId = contestant.Id;

    SubmittedByPerson = submittedByPerson;
    SubmittedByPersonId = submittedByPerson?.Id;

    SourceKind = sourceKind;
  }

  private DraftPredictionSet()
  {
  }

  public string PublicId { get; private set; } = default!;

  public PredictionSeason Season { get; private set; } = default!;
  public PredictionSeasonId SeasonId { get; private set; } = default!; 

  public DraftPart DraftPart { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;          // where rules live 

  public PredictionContestant Contestant { get; private set; } = default!;
  public ContestantId ContestantId { get; private set; } = default!;  

  public Person? SubmittedByPerson { get; private set; } = default!; 
  public PersonId? SubmittedByPersonId { get; private set; } = default!;

  public DateTime SubmittedAtUtc { get; private set; } = DateTime.UtcNow;
  public PredictionSourceKind SourceKind { get; private set; } = default!; // UI, TextUpload, API
  public DateTime? LockedAtUtc { get; private set; } = default!;

  // Snapshot of the rules AT LOCK (audit/repro). Used only for display; actual scoring
  // should pull the authoritative rules from DraftPartPredictionRules at finalize time.
  public PredictionRulesSnapshot? RulesSnapshot { get; private set; } = default!;

  public bool IsLocked => LockedAtUtc.HasValue;

  public IReadOnlyCollection<PredictionEntry> Entries => _entries;
  public IReadOnlyCollection<SurrogateAssignment> Surrogates => _surrogates;

  public static Result<DraftPredictionSet> Create(
    string publicId,
    PredictionSeason season,
    DraftPart draftPart,
    PredictionContestant contestant,
    Person? submittedByPerson,
    PredictionSourceKind sourceKind)
  {
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(draftPart);
    ArgumentNullException.ThrowIfNull(contestant);

    var set = new DraftPredictionSet(
      publicId: publicId,
      season: season,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: submittedByPerson,
      sourceKind: sourceKind);

    set.Raise(new PredictionSetSubmittedDomainEvent(
      setId: set.Id,
      contestantId: set.ContestantId,
      draftPartId: set.DraftPartId,
      seasonId: set.SeasonId));

    return set;
  }

  /// <summary>
  /// Replaces all entries. Rejected if the set is already locked.
  /// </summary>
  /// <param name="newEntries">The new entries to replace the existing ones.</param>
  /// <returns>A result indicating success or failure.</returns>
  public Result ReplaceEntries(IEnumerable<PredictionEntry> newEntries)
  {
    if (IsLocked)
    {
      return Result.Failure(PredictionErrors.SetAlreadyLocked);
    }

    _entries.Clear();
    _entries.AddRange(newEntries);

    return Result.Success();
  }

  /// <summary>
  /// Locks the set and captures a rules snapshot. Once locked, entries cannot be replaced.
  /// </summary>
  /// <param name="rulesSnapshot">The snapshot of the rules at the time of locking.</param>
  /// <param name="now">The current date and time.</param>
  /// <returns>A result indicating success or failure.</returns>
  public Result Lock(PredictionRulesSnapshot rulesSnapshot, DateTime now)
  {
    if (IsLocked)
    {
      return Result.Failure(PredictionErrors.SetAlreadyLocked);
    }

    RulesSnapshot = rulesSnapshot;
    LockedAtUtc = now;

    Raise(new PredictionSetLockedDomainEvent(
      setId: Id,
      contestantId: ContestantId,
      draftPartId: DraftPartId,
      lockedAtUtc: now));

    return Result.Success();
  }

  /// <summary>
  /// Attaches a surrogate assignment to this primary set.
  /// </summary>
  /// <param name="surrogate">The surrogate assignment to attach.</param>
  /// <returns>A result indicating success or failure.</returns>
  public Result AttachSurrogate(SurrogateAssignment surrogate)
  {
    ArgumentNullException.ThrowIfNull(surrogate);

    if (surrogate.PrimarySetId != Id)
    {
      return Result.Failure(PredictionErrors.SurrogatePrimarySetMismatch);
    }

    _surrogates.Add(surrogate);
    return Result.Success();
  }

  public void RaiseScored(int pointsAwarded, bool shootTheMoon, PredictionSeasonId seasonId)
  {
    Raise(new PredictionSetScoredDomainEvent(
      setId: Id,
      contestantId: ContestantId,
      pointsAwarded: pointsAwarded,
      shootTheMoon: shootTheMoon,
      seasonId: seasonId));
  }

}
