namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftSummary;

internal sealed class UpsertDraftSummaryCommandHandler(IDraftReportingRepository repository)
  : ICommandHandler<UpsertDraftSummaryCommand>
{
  private readonly IDraftReportingRepository _repository = repository;

  public async Task<Result> Handle(
    UpsertDraftSummaryCommand request,
    CancellationToken cancellationToken
  )
  {
    var existingDraftSummary = await _repository.GetDraftSummaryAsync(
      request.DraftId,
      request.DraftPartPublicId,
      cancellationToken
    );

    if (existingDraftSummary is null)
    {
      var summary = DraftSummary.Create(
        draftId: request.DraftId,
        draftPublicId: request.DraftPublicId,
        draftPartPublicId: request.DraftPartPublicId,
        title: request.Title,
        draftType: request.DraftType,
        partIndex: request.PartIndex,
        totalParts: request.TotalParts,
        totalPicks: request.TotalPicks,
        isPatreon: request.IsPatreon,
        episodeNumber: request.EpisodeNumber,
        isComplete: false,
        completedAtUtc: null,
        createdAtUtc: DateTime.UtcNow
      );
      _repository.AddDraftSummary(summary);
    }
    else
    {
      existingDraftSummary.Update(
        totalParts: request.TotalParts,
        totalPicks: request.TotalPicks,
        episodeNumber: request.EpisodeNumber,
        isPatreon: request.IsPatreon
      );

      _repository.UpdateDraftSummary(existingDraftSummary);
    }

    if (request.VetoCount > 0)
    {
      var stats = await _repository.GetSiteStatsAsync(cancellationToken);

      if (stats is not null)
      {
        stats.IncrementVetoes(request.VetoCount);
        _repository.UpdateSiteStats(stats);
      }
    }

    return Result.Success();
  }
}
