namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftPartRelease;

internal sealed class UpsertDraftPartReleaseCommandHandler(
  IDraftReportingRepository repository,
  ICacheService cacheService
) : ICommandHandler<UpsertDraftPartReleaseCommand>
{
  private readonly IDraftReportingRepository _repository = repository;
  private readonly ICacheService _cacheService = cacheService;

  public async Task<Result> Handle(
    UpsertDraftPartReleaseCommand request,
    CancellationToken cancellationToken
  )
  {
    var existingRelease = await _repository.GetDraftPartReleaseAsync(
      request.DraftPartPublicId,
      request.ReleaseChannel,
      cancellationToken
    );

    if (existingRelease is null)
    {
      var draftPartRelease = DraftPartRelease.Create(
        draftId: request.DraftId,
        draftPartPublicId: request.DraftPartPublicId,
        releaseChannel: request.ReleaseChannel,
        releaseDate: request.ReleaseDate
      );

      _repository.AddDraftPartRelease(draftPartRelease);
    }
    else
    {
      existingRelease.UpdateReleaseDate(request.ReleaseDate);

      _repository.UpdateDraftPartRelease(existingRelease);
    }

    if (request.ReleaseChannel == "MainFeed" && request.EpisodeNumber.HasValue)
    {
      var summary = await _repository.GetDraftSummaryAsync(
        request.DraftId,
        request.DraftPartPublicId,
        cancellationToken
      );

      if (summary is not null)
      {
        summary.SetEpisodeNumber(request.EpisodeNumber.Value);
        _repository.UpdateDraftSummary(summary);
      }
    }

    if (request.ReleaseChannel == "MainFeed")
    {
      // A main-feed release can make a policy 2 draft canonical, which changes the Record Book.
      // The unit of work commits after this handler returns, so the Record Book also has a short
      // cache lifetime to bound any stale read in the gap.
      await _cacheService.RemoveAsync(
        ReportingCacheKeys.RecordBookCanonicalCacheKey,
        cancellationToken
      );
      await _cacheService.RemoveAsync(ReportingCacheKeys.RecordBookAllCacheKey, cancellationToken);
    }

    return Result.Success();
  }
}
