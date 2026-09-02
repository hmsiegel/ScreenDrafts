namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetFixedBoardLayout;

internal sealed class SetFixedBoardLayoutCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<SetFixedBoardLayoutCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SetFixedBoardLayoutCommand request,
    CancellationToken cancellationToken
  )
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

    var result = guestDraft.UseFixedBoardLayout(_ =>
      _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraftPosition)
    );

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}
