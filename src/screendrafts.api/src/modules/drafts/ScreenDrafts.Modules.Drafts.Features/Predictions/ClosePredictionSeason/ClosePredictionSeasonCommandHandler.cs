namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ClosePredictionSeason;

internal sealed class ClosePredictionSeasonCommandHandler(
  IPredictionSeasonRepository seasonRepository)
  : ICommandHandler<ClosePredictionSeasonCommand>
{
  private readonly IPredictionSeasonRepository _seasonRepository = seasonRepository;

  public async Task<Result> Handle(
    ClosePredictionSeasonCommand request,
    CancellationToken cancellationToken)
  {
    var season = await _seasonRepository.GetByPublicIdAsync(
      request.SeasonPublicId,
      cancellationToken);

    if (season is null)
    {
      return Result.Failure(PredictionErrors.SeasonNotFound(request.SeasonPublicId));
    }

    if (season.IsClosed)
    {
      return Result.Failure(PredictionErrors.SeasonAlreadyClosed);
    }

    season.CloseSeason(request.EndsOn);

    _seasonRepository.Update(season);

    return Result.Success();
  }
}
