namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoVeto;

internal sealed class UndoVetoCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi
) : ICommandHandler<UndoVetoCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(UndoVetoCommand request, CancellationToken cancellationToken)
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (guestDraft is null)
    {
      return Result.Failure(GuestDraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    if (caller.UserId != guestDraft.OwnerUserId)
    {
      return Result.Failure(GuestDraftErrors.OnlyOwnerCanPerformThisAction);
    }

    if (guestDraft.GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.DraftNotStarted);
    }

    var pick = guestDraft.Picks.FirstOrDefault(p => p.PlayOrder == request.PlayOrder);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFoundByPlayOrder(request.PlayOrder));
    }

    var result = guestDraft.UndoVeto(pick.Id);

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}
