namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class PredictionStanding : Entity<PredictionStandingId>
{
  private PredictionStanding(
    PredictionSeason season,
    PredictionContestant contestant,
    PredictionStandingId? id = null)
    : base(id ?? PredictionStandingId.CreateUnique())
  {
    Season = season;
    SeasonId = season.Id;
    Contestant = contestant;
    ContestantId = contestant.Id;
  }

  private PredictionStanding()
  {
  }

  public PredictionSeason Season { get; private set; } = default!;
  public PredictionSeasonId SeasonId { get; private set; } = default!;

  public PredictionContestant Contestant { get; private set; } = default!;
  public ContestantId ContestantId { get; private set; } = default!;

  public decimal Points { get; private set; } = default!;
  public DateTime? FirstCrossedTargetAtUtc { get; private set; } = default!;
  // methods: Add(points, targetPoints, beforeTotal, now)

  public static PredictionStanding Create(
    PredictionSeason season,
    PredictionContestant contestant)
  {
    ArgumentNullException.ThrowIfNull(season);
    ArgumentNullException.ThrowIfNull(contestant);
    return new PredictionStanding(season, contestant);
  }

  public void Add(decimal points, int targetPoints, decimal beforeTotal, DateTime now)
  {
    Points += points;
    if (beforeTotal < targetPoints && Points >= targetPoints)
    {
      FirstCrossedTargetAtUtc = now;
    }
  }
}
