namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVeto;

internal sealed class ApplyVetoCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi
) : ICommandHandler<ApplyVetoCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(ApplyVetoCommand request, CancellationToken cancellationToken)
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

    var issuer = guestDraft.Participants.FirstOrDefault(p => p.UserId == caller.UserId);

    if (issuer is null)
    {
      return Result.Failure(GuestDraftErrors.CallerNotAParticipant);
    }

    var result = guestDraft.ApplyVeto(pick.Id, issuer.Id.Value, issuer.PublicId, request.Note);

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}
