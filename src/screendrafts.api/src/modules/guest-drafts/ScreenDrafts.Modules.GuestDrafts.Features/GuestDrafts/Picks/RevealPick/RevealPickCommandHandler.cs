using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.RevealPick;

internal sealed class RevealPickCommandHandler(
  IDraftRepository guestDraftRepository,
  IDrafterRepository guestDrafterRepository,
  IUsersApi usersApi
) : ICommandHandler<RevealPickCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(RevealPickCommand request, CancellationToken cancellationToken)
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (guestDraft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.GuestDraftPublicId));
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
      return Result.Failure(DrafterErrors.NotFoundForUser(caller.UserId));
    }

    var revealer = guestDraft.FindByParticipantRef(Participant.From(callerDrafter.Id));

    if (revealer is null)
    {
      return Result.Failure(DraftErrors.CallerNotAParticipant);
    }

    // GuestDraft.RevealPick checks Status != InProgress before it ever looks for
    // the pick -- same handler-ordering fix as UndoVeto's.
    if (guestDraft.GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    var pick = guestDraft.Picks.FirstOrDefault(p => p.PlayOrder == request.PlayOrder);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFoundByPlayOrder(request.PlayOrder));
    }

    // Mirrors canonical RevealPickCommandHandler's hostless branch exactly -- every
    // guest draft is hostless, so this check always applies, no primary-host branch.
    if (!pick.IsRevealAuthorized(revealer.Id.Value))
    {
      return Result.Failure(DraftErrors.NotRevealAuthorized);
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
