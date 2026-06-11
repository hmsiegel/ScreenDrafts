namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ActivateSpotlight;

internal sealed class ActivateSpotlightCommandHandler(
  IDraftReportingRepository repository,
  ICacheService cacheService
) : ICommandHandler<ActivateSpotlightCommand>
{
  private readonly IDraftReportingRepository _repository = repository;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<Result> Handle(
    ActivateSpotlightCommand request,
    CancellationToken cancellationToken
  )
  {
    var target = await _repository.GetSpotlightByPublicIdAsync(request.PublicId, cancellationToken);

    if (target is null)
    {
      return Result.Failure(DraftReportingErrors.SpotlightNotFound(request.PublicId));
    }

    if (target.IsActive)
    {
      return Result.Success();
    }

    // Deactivate current active spotlight (if any) in the same unit of work.
    var current = await _repository.GetActiveSpotlightAsync(cancellationToken);
    if (current is not null)
    {
      current.Deactivate();
      current.Unpin();
    }

    target.Activate();
    target.Pin();

    // Cache invalidated after UoW commits via the pipeline behavior,
    // but we clear here pre-emptively so the next read is always fresh.
    await _cacheService.RemoveAsync(ReportingCacheKeys.SpotlightCacheKey, cancellationToken);

    return Result.Success();
  }
}
