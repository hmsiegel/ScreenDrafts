namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AddCarryover;

internal sealed class AddCarryoverCommandHandler(
  IPredictionSeasonRepository seasonRepository,
  IPredictionContestantRepository contestantRepository)
  : ICommandHandler<AddCarryoverCommand>
{
  private readonly IPredictionSeasonRepository _seasonRepository = seasonRepository;
  private readonly IPredictionContestantRepository _contestantRepository = contestantRepository;

  public async Task<Result> Handle(
    AddCarryoverCommand request,
    CancellationToken cancellationToken)
  {
    var season = await _seasonRepository.GetByPublicIdAsync(
      request.SeasonPublicId,
      cancellationToken);

    if (season is null)
    {
      return Result.Failure(PredictionErrors.SeasonNotFound(request.SeasonPublicId));
    }

    var contestant = await _contestantRepository.GetByPublicIdAsync(
      request.ContestantPublicId,
      cancellationToken);

    if (contestant is null)
    {
      return Result.Failure(PredictionErrors.ContestantNotFound(request.ContestantPublicId));
    }

    var kind = CarryoverKind.FromValue(request.Kind);

    var carryover = PredictionCarryover.Create(
      season: season,
      contestant: contestant,
      points: request.Points,
      carryoverKind: kind,
      reason: request.Reason);

    season.AddCarryover(carryover);

    _seasonRepository.Update(season);

    return Result.Success();
  }
}
