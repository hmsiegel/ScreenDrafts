namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetSchedule;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class SetDraftScheduleCommandHandler(
  IDraftRepository draftRepository,
  IUsersApi usersApi
) : ICommandHandler<SetDraftScheduleCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(
    SetDraftScheduleCommand request,
    CancellationToken cancellationToken
  )
  {
    var draft = await _draftRepository.GetByPublicIdAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    if (caller.UserId != draft.OwnerUserId)
    {
      return Result.Failure(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var result = draft.SetScheduledForUtc(request.ScheduledForUtc);

    if (result.IsFailure)
    {
      return result;
    }

    _draftRepository.Update(draft);

    return Result.Success();
  }
}
