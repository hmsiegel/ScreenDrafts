namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed class SetGuestDraftStatusCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi
) : ICommandHandler<SetGuestDraftStatusCommand, SetGuestDraftStatusResponse>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result<SetGuestDraftStatusResponse>> Handle(
    SetGuestDraftStatusCommand request,
    CancellationToken cancellationToken)
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken);

    if (guestDraft is null)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(
        GuestDraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, ct);

    if (caller is null)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    if (caller.UserId != guestDraft.OwnerUserId)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(GuestDraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var result = request.Action switch
    {
      GuestDraftStatusAction.Start => guestDraft.Start(),
      GuestDraftStatusAction.Complete => guestDraft.Complete(),
      _ => Result.Failure(GuestDraftErrors.InvalidStatusAction),
    };

    if (result.IsFailure)
    {
      return Result.Failure<SetGuestDraftStatusResponse>(result.Errors);
    }

    _guestDraftRepository.Update(guestDraft);

    return Result.Success(new SetGuestDraftStatusResponse
    {
      GuestDraftPublicId = guestDraft.PublicId,
      Status = guestDraft.GuestDraftStatus.Name
    });
  }
}
