namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.RemoveParticipant;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class RemoveParticipantCommandHandler(
  IDraftRepository draftRepository,
  IDrafterRepository drafterRepository,
  IUsersApi usersApi
) : ICommandHandler<RemoveParticipantCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IDrafterRepository _drafterRepository = drafterRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(
    RemoveParticipantCommand request,
    CancellationToken cancellationToken
  )
  {
    // Loads the board too: removal frees the participant's position.
    var draft = await _draftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    if (caller.UserId != draft.OwnerUserId)
    {
      return Result.Failure(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var drafter = await _drafterRepository.GetByPublicIdAsync(
      request.GuestDrafterPublicId,
      cancellationToken
    );

    if (drafter is null)
    {
      return Result.Failure(DrafterErrors.NotFound(request.GuestDrafterPublicId));
    }

    var result = draft.RemoveParticipant(Participant.From(drafter.Id));

    if (result.IsFailure)
    {
      return result;
    }

    _draftRepository.Update(draft);

    return Result.Success();
  }
}
