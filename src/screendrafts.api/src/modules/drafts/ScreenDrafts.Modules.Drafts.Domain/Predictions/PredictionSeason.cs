namespace ScreenDrafts.Modules.Drafts.Domain.Predictions;

public sealed class PredictionSeason : AggregateRoot<PredictionSeasonId, Guid>
{
  private readonly List<PredictionStanding> _standings = [];
  private readonly List<PredictionCarryover> _carryovers = [];
  private readonly List<DraftPredictionSet> _sets = [];

  private PredictionSeason(
    int number,
    DateOnly startsOn,
    string publicId,
    PredictionSeasonId? id = null)
    : base(id ?? PredictionSeasonId.CreateUnique())
  {
    Number = number;
    StartsOn = startsOn;
    PublicId = publicId;
  }

  private PredictionSeason()
  {
  }

  public int Number { get; private set; } = 1;
  public string PublicId { get; private set; } = default!;
  public DateOnly StartsOn { get; private set; } = default!;
  public DateOnly? EndsOn { get; private set; } = default!;
  public int TargetPoints { get; private set; } = 100;
  public bool IsClosed { get; private set; } = default!;

  public IReadOnlyCollection<PredictionStanding> Standings => _standings.AsReadOnly();
  public IReadOnlyCollection<PredictionCarryover> Carryovers => _carryovers.AsReadOnly();
  public IReadOnlyCollection<DraftPredictionSet> Sets => _sets.AsReadOnly();

  public static PredictionSeason Create(
    int number,
    DateOnly startsOn,
    string publicId) => new(
      number: number,
      startsOn: startsOn,
      publicId: publicId);

  /// <summary>
  /// Closes the season and records the date.
  /// </summary>
  /// <param name="endsOn">The date the season ends.</param>
  public void CloseSeason(DateOnly endsOn)
  {
    EndsOn = endsOn;
    IsClosed = true;
  }

  /// <summary>
  /// Records a points carryover (handicap, bonus, or manual adjustment) for a contestant in the season.
  /// </summary>
  /// <param name="carryover">The carryover to record.</param>
  public void AddCarryover(PredictionCarryover carryover)
  {
    ArgumentNullException.ThrowIfNull(carryover);
    _carryovers.Add(carryover);
  }
}
