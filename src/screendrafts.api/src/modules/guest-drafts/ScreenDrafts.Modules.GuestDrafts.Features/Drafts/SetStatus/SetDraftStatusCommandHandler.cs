namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetStatus;

internal sealed class SetDraftStatusCommandHandler(
  IDraftRepository draftRepository,
  IUsersApi usersApi
) : ICommandHandler<SetDraftStatusCommand, SetGuestDraftStatusResponse>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result<SetGuestDraftStatusResponse>> Handle(
    SetDraftStatusCommand request,
    CancellationToken cancellationToken
  )
  {
    var draft = await _draftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (draft is null)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(
        DraftErrors.NotFound(request.GuestDraftPublicId)
      );
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId)
      );
    }

    if (caller.UserId != draft.OwnerUserId)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var result = request.Action switch
    {
      DraftStatusAction.Start => draft.Start(),
      DraftStatusAction.Complete => draft.Complete(),
      _ => Result.Failure(DraftErrors.InvalidStatusAction),
    };

    if (result.IsFailure)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(result.Errors);
    }

    _draftRepository.Update(draft);

    return Result.Success(
      new SetGuestDraftStatusResponse
      {
        GuestDraftPublicId = draft.PublicId,
        Status = draft.GuestDraftStatus.Name,
      }
    );
  }
}
