namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetSchedule;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class SetDraftPartScheduleCommandHandler(IDraftPartRepository draftPartRepository)
  : ICommandHandler<SetDraftPartScheduleCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(
    SetDraftPartScheduleCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var result = draftPart.SetScheduledFor(request.ScheduledForUtc);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
