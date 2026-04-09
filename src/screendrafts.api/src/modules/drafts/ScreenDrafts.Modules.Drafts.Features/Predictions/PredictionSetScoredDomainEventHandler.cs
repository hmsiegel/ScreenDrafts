using ScreenDrafts.Modules.Drafts.Domain.Predictions.DomainEvents;
using ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

namespace ScreenDrafts.Modules.Drafts.Features.Predictions;

internal sealed class PredictionSetScoredDomainEventHandler(
  IPredictionStandingRepository predictionStandingRepository,
  IPredictionSeasonRepository predictionSeasonRepository,
  IPredictionContestantRepository predictionContestantRepository,
  IUnitOfWork unitOfWork,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<PredictionSetScoredDomainEvent>
{
  private readonly IPredictionStandingRepository _predictionStandingRepository = predictionStandingRepository;
  private readonly IPredictionSeasonRepository _predictionSeasonRepository = predictionSeasonRepository;
  private readonly IPredictionContestantRepository _predictionContestantRepository = predictionContestantRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(PredictionSetScoredDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    var season = await _predictionSeasonRepository.GetByIdAsync(
      id: domainEvent.SeasonId,
      cancellationToken: cancellationToken);

    if (season is null)
    {
      return;
    }

    var contestant = await _predictionContestantRepository.GetByIdAsync(
      id: domainEvent.ContestantId,
      cancellationToken: cancellationToken);

    if (contestant is null)
    {
      return;
    }

    var standing = await _predictionStandingRepository.GetByContestantAndSeasonAsync(
      seasonId: domainEvent.SeasonId,
      contestantId: domainEvent.ContestantId,
      cancellationToken: cancellationToken);

    if (standing is null)
    {
      standing = PredictionStanding.Create(
        season: season,
        contestant: contestant);

      _predictionStandingRepository.Add(standing);
    }

    var beforeTotal = standing.Points;

    standing.Add(
      points: domainEvent.PointsAwarded,
      targetPoints: season.TargetPoints,
      beforeTotal: beforeTotal,
      now: _dateTimeProvider.UtcNow);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
