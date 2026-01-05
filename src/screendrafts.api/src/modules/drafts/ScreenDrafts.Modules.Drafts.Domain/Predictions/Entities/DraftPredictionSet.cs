using ScreenDrafts.Modules.Drafts.Domain.Predictions.Enums;

namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class DraftPredictionSet : Entity<DraftPredictionSetId>
{
  private readonly List<PredictionEntry> _entries = [];
  private readonly List<SurrogateAssignment> _surrogates = [];

  private DraftPredictionSet(
    PredictionSeason season,
    DraftPart draftPart,
    PredictionContestant contestant,
    Person? submittedByPerson,
    PredictionSourceKind sourceKind,
    DraftPredictionSetId? id = null)
    : base(id ?? DraftPredictionSetId.CreateUnique())
  {
    Season = season;
    SeasonId = season.Id;

    DraftPart = draftPart;
    DraftPartId = draftPart.Id;

    Conttestant = contestant;
    ContestantId = contestant.Id;

    SubmittedByPerson = submittedByPerson;
    SubmittedByPersonId = submittedByPerson?.Id;

    SourceKind = sourceKind;
  }

  private DraftPredictionSet()
  {
  }

  public PredictionSeason Season { get; private set; } = default!;
  public PredictionSeasonId SeasonId { get; private set; } = default!; 

  public DraftPart DraftPart { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;          // where rules live 

  public PredictionContestant Conttestant { get; private set; } = default!;
  public ContestantId ContestantId { get; private set; } = default!;  

  public Person? SubmittedByPerson { get; private set; } = default!; 
  public PersonId? SubmittedByPersonId { get; private set; } = default!;

  public DateTime SubmittedAtUtc { get; private set; } = DateTime.UtcNow;
  public PredictionSourceKind SourceKind { get; private set; } = default!; // UI, TextUpload, API
  public DateTime? LockedAtUtc { get; private set; } = default!;

  // Snapshot of the rules AT LOCK (audit/repro). Used only for display; actual scoring
  // should pull the authoritative rules from DraftPartPredictionRules at finalize time.
  public PredictionRulesSnapshot RulesSnapshot { get; private set; } = default!;

  public IReadOnlyCollection<PredictionEntry> Entries => _entries;
  public IReadOnlyCollection<SurrogateAssignment> Surrogates => _surrogates;

  public static DraftPredictionSet Create(
    PredictionSeason season,
    DraftPart draftPart,
    PredictionContestant contestant,
    Person? submittedByPerson,
    PredictionSourceKind sourceKind)
  {
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(draftPart);
    ArgumentNullException.ThrowIfNull(contestant);

    return new DraftPredictionSet(
      season: season,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: submittedByPerson,
      sourceKind: sourceKind);
  }

  // methods: ReplaceEntries(...), Lock(...), AttachSurrogate(...)
}
