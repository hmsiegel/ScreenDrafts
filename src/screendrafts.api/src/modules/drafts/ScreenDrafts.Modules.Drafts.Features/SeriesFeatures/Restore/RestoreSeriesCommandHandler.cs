namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Restore;

internal sealed class RestoreSeriesCommandHandler(
  ISeriesRepository seriesRepository,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<RestoreSeriesCommand>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    RestoreSeriesCommand request,
    CancellationToken cancellationToken
  )
  {
    var series = await _seriesRepository.GetSeriesByPublicIdIncludingDeletedAsync(
      request.PublicId,
      cancellationToken
    );

    if (series is null)
    {
      return Result.Failure(SeriesErrors.SeriesNotFound(request.PublicId));
    }

    var restoreResult = series.Restore(_dateTimeProvider.UtcNow);

    if (restoreResult.IsFailure)
    {
      return restoreResult;
    }

    _seriesRepository.Update(series);

    return Result.Success();
  }
}
