namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.AssignParticipantToPosition;

internal sealed class AssignParticipantToPositionCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi
) : ICommandHandler<AssignParticipantToPositionCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
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

    var participant = guestDraft.Participants.FirstOrDefault(p =>
      p.PublicId == request.ParticipantPublicId
    );

    if (participant is null)
    {
      return Result.Failure(GuestDraftErrors.ParticipantNotFound(request.ParticipantPublicId));
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
