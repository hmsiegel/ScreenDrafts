using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Positions.AssignParticipantToPosition;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Positions.AssignParticipantToPosition;

internal sealed class AssignParticipantToPositionCommandHandler(
  IDraftRepository guestDraftRepository,
  IDrafterRepository guestDrafterRepository,
  IUsersApi usersApi
) : ICommandHandler<AssignParticipantToPositionCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(
    AssignParticipantToPositionCommand request,
    CancellationToken cancellationToken
  )
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

    if (caller.UserId != guestDraft.OwnerUserId)
    {
      return Result.Failure(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var position = guestDraft.GameBoard?.Positions.FirstOrDefault(p =>
      p.PublicId == request.PositionPublicId
    );

    if (position is null)
    {
      return Result.Failure(DraftErrors.PositionDoesNotBelongToThisBoard);
    }

    var guestDrafter = await _guestDrafterRepository.GetByPublicIdAsync(
      request.GuestDrafterPublicId,
      cancellationToken
    );

    if (guestDrafter is null)
    {
      return Result.Failure(DrafterErrors.NotFound(request.GuestDrafterPublicId));
    }

    // Must already be an added participant of THIS draft (via AddParticipant)
    // -- being a registered GuestDrafter isn't enough on its own.
    var participant = guestDraft.FindByParticipantRef(Participant.From(guestDrafter.Id));

    if (participant is null)
    {
      return Result.Failure(DraftErrors.ParticipantNotFound(request.GuestDrafterPublicId));
    }

    var result = guestDraft.AssignParticipantToPosition(position, participant.Id.Value);

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}
