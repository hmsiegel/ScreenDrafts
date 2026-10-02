namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpdateSpotlight;

// ── Handler ───────────────────────────────────────────────────────────────

internal sealed class UpdateSpotlightCommandHandler(
  IDraftReportingRepository repository,
  ICacheService cacheService
) : ICommandHandler<UpdateSpotlightCommand>
{
  private readonly IDraftReportingRepository _repository = repository;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<Result> Handle(
    UpdateSpotlightCommand request,
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

    Uri? spotifyUri = string.IsNullOrWhiteSpace(request.SpotifyUrl)
      ? null
      : new Uri(request.SpotifyUrl);

    spotlight.UpdateDescription(request.SpotlightDescription);
    spotlight.UpdateSpotifyUrl(spotifyUri);

    // The home page hero reads the active spotlight from cache.
    if (spotlight.IsActive)
    {
      await _cacheService.RemoveAsync(ReportingCacheKeys.SpotlightCacheKey, cancellationToken);
    }

    return Result.Success();
  }
}
