namespace ScreenDrafts.Modules.Drafts.Domain.Predictions;

public sealed class PredictionSeason : AggregateRoot<PredictionSeasonId, Guid>
{
  private readonly List<PredictionStanding> _standings = [];
  private readonly List<PredictionCarryover> _carryovers = [];
  private readonly List<DraftPredictionSet> _sets = [];

  private PredictionSeason(
    int number,
    DateOnly startsOn,
    PredictionSeasonId? id = null)
    : base(id ?? PredictionSeasonId.CreateUnique())
  {
    Number = number;
    StartsOn = startsOn;
  }

  private PredictionSeason()
  {
  }

  public int Number { get; private set; } = 1;
  public DateOnly StartsOn { get; private set; } = default!;
  public DateOnly? EndsOn { get; private set; } = default!;
  public int TargetPoints { get; private set; } = 100;
  public bool IsClosed { get; private set; } = default!;

  public IReadOnlyCollection<PredictionStanding> Standings => _standings.AsReadOnly();
  public IReadOnlyCollection<PredictionCarryover> Carryovers => _carryovers.AsReadOnly();
  public IReadOnlyCollection<DraftPredictionSet> Sets => _sets.AsReadOnly();

  public static PredictionSeason Create(
    int number,
    DateOnly startsOn)
  {
    return new PredictionSeason(
      number: number,
      startsOn: startsOn);
  }

  public void CloseSeason()
  {
    IsClosed = true;
  }
}
