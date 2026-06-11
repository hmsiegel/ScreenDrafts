namespace ScreenDrafts.Modules.Reporting.Features.Drafts.DeactivateSpotlight;

internal sealed class DeactivateSpotlightCommandHandler(
  IDraftReportingRepository repository,
  ICacheService cacheService
) : ICommandHandler<DeactivateSpotlightCommand>
{
  private readonly IDraftReportingRepository _repository = repository;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<Result> Handle(
    DeactivateSpotlightCommand request,
    CancellationToken cancellationToken
  )
  {
    var spotlight = await _repository.GetSpotlightByPublicIdAsync(
      request.PublicId,
      cancellationToken
    );

    if (spotlight is null)
    {
      return Result.Failure(DraftReportingErrors.SpotlightNotFound(request.PublicId));
    }

    if (!spotlight.IsActive)
    {
      return Result.Success();
    }

    spotlight.Deactivate();
    spotlight.Unpin();

    await _cacheService.RemoveAsync(ReportingCacheKeys.SpotlightCacheKey, cancellationToken);

    return Result.Success();
  }
}
