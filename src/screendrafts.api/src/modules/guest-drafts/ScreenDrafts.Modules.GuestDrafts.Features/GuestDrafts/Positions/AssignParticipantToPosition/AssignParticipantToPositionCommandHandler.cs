namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.AssignParticipantToPosition;

internal sealed class AssignParticipantToPositionCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IGuestDrafterRepository guestDrafterRepository,
  IUsersApi usersApi
) : ICommandHandler<AssignParticipantToPositionCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IGuestDrafterRepository _guestDrafterRepository = guestDrafterRepository;
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

    var position = guestDraft.GameBoard?.Positions.FirstOrDefault(p =>
      p.PublicId == request.PositionPublicId
    );

    if (position is null)
    {
      return Result.Failure(GuestDraftErrors.PositionDoesNotBelongToThisBoard);
    }

    var guestDrafter = await _guestDrafterRepository.GetByPublicIdAsync(
      request.GuestDrafterPublicId,
      cancellationToken
    );

    if (guestDrafter is null)
    {
      return Result.Failure(GuestDrafterErrors.NotFound(request.GuestDrafterPublicId));
    }

    // Must already be an added participant of THIS draft (via AddParticipant)
    // -- being a registered GuestDrafter isn't enough on its own.
    var participant = guestDraft.FindByParticipantRef(GuestParticipant.From(guestDrafter.Id));

    if (participant is null)
    {
      return Result.Failure(GuestDraftErrors.ParticipantNotFound(request.GuestDrafterPublicId));
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
