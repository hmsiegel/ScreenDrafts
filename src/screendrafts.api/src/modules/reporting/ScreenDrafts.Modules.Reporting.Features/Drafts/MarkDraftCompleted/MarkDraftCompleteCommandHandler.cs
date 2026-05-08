namespace ScreenDrafts.Modules.Reporting.Features.Drafts.MarkDraftCompleted;

internal sealed class MarkDraftCompleteCommandHandler(
  IDraftReportingRepository draftReportingRepository
) : ICommandHandler<MarkDraftCompleteCommand>
{
  public async Task<Result> Handle(
    MarkDraftCompleteCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftSummaries = await draftReportingRepository.GetDraftSummariesByDraftIdAsync(
      request.DraftId,
      cancellationToken
    );

    if (!draftSummaries.Any())
    {
      return Result.Failure(DraftReportingErrors.NotFound(request.DraftId));
    }
    foreach (var draftSummary in draftSummaries)
    {
      draftSummary.MarkComplete();
      draftReportingRepository.UpdateDraftSummary(draftSummary);
    }

    return Result.Success();
  }
}
