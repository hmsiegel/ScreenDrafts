namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class PredictionCarryover : Entity<PredictionCarryoverId>
{
  private PredictionCarryover(
    PredictionSeason season,
    PredictionContestant contestant,
    decimal points,
    string? reason = null,
    PredictionCarryoverId? id = null)
    : base(id ?? PredictionCarryoverId.CreateUnique())
  {
    Season = Guard.Against.Null(season);
    SeasonId = season.Id;

    Contestant = Guard.Against.Null(contestant);
    ContestantId = contestant.Id;

    Points = points;
    Reason = reason;
  }
  
  private PredictionCarryover()
  {
  }

  public PredictionSeason Season { get; private set; } = default!;
  public PredictionSeasonId SeasonId { get; private set; } = default!;

  public  PredictionContestant Contestant { get; private set; } = default!;
  public ContestantId ContestantId { get; private set; } = default!;

  public decimal Points { get; private set; } = 0;
  public string? Reason { get; private set; } = string.Empty;

  public static PredictionCarryover Create(
    PredictionSeason season,
    PredictionContestant contestant,
    decimal points,
    string? reason = null)
  {
    return new PredictionCarryover(
      season,
      contestant,
      points,
      reason);
  }
}
