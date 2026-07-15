namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Delete;

internal sealed class DeleteSeriesCommandHandler(
  ISeriesRepository seriesRepository,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<DeleteSeriesCommand>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(DeleteSeriesCommand request, CancellationToken cancellationToken)
  {
    var series = await _seriesRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
    if (series is null)
    {
      return Result.Failure(SeriesErrors.SeriesNotFound(request.PublicId));
    }
    var deleteResult = series.SoftDelete(_dateTimeProvider.UtcNow);

    if (deleteResult.IsFailure)
    {
      return deleteResult;
    }

    _seriesRepository.Update(series);

    return Result.Success();
  }
}
