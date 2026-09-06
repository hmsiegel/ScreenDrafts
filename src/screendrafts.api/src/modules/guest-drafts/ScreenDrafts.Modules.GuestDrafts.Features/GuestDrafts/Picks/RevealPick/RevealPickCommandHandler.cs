namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.RevealPick;

internal sealed class RevealPickCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IGuestDrafterRepository guestDrafterRepository,
  IUsersApi usersApi
) : ICommandHandler<RevealPickCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IGuestDrafterRepository _guestDrafterRepository = guestDrafterRepository;
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

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    var callerDrafter = await _guestDrafterRepository.GetByUserIdAsync(
      caller.UserId,
      cancellationToken
    );

    if (callerDrafter is null)
    {
      return Result.Failure(GuestDrafterErrors.NotFoundForUser(caller.UserId));
    }

    var revealer = guestDraft.FindByParticipantRef(GuestParticipant.From(callerDrafter.Id));

    if (revealer is null)
    {
      return Result.Failure(GuestDraftErrors.CallerNotAParticipant);
    }

    // GuestDraft.RevealPick checks Status != InProgress before it ever looks for
    // the pick -- same handler-ordering fix as UndoVeto's.
    if (guestDraft.GuestDraftStatus != GuestDraftStatus.InProgress)
    {
      return Result.Failure(GuestDraftErrors.DraftNotStarted);
    }

    var pick = guestDraft.Picks.FirstOrDefault(p => p.PlayOrder == request.PlayOrder);

    if (pick is null)
    {
      return Result.Failure(GuestDraftErrors.PickNotFoundByPlayOrder(request.PlayOrder));
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
