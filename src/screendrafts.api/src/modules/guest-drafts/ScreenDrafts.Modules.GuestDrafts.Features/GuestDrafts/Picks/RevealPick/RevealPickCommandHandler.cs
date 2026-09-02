namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.RevealPick;

internal sealed class RevealPickCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi
) : ICommandHandler<RevealPickCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(RevealPickCommand request, CancellationToken cancellationToken)
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (guestDraft is null)
    {
      return Result.Failure(GuestDraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var pick = guestDraft.Picks.FirstOrDefault(p => p.PlayOrder == request.PlayOrder);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFoundByPlayOrder(request.PlayOrder));
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    var revealer = guestDraft.Participants.FirstOrDefault(p => p.UserId == caller.UserId);

    if (revealer is null)
    {
      return Result.Failure(GuestDraftErrors.CallerNotAParticipant);
    }

    // Mirrors canonical RevealPickCommandHandler's hostless branch exactly -- every
    // guest draft is hostless, so this check always applies, no primary-host branch.
    if (!pick.IsRevealAuthorized(revealer.Id.Value))
    {
      return Result.Failure(GuestDraftErrors.NotRevealAuthorized);
    }

    var result = guestDraft.RevealPick(pick.Id);

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}
