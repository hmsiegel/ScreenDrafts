namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Resend;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class ResendGuestDraftInvitationCommandHandler(
  IDraftRepository draftRepository,
  IInvitationRepository invitationRepository,
  IUsersApi usersApi,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<ResendGuestDraftInvitationCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IInvitationRepository _invitationRepository = invitationRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    ResendGuestDraftInvitationCommand request,
    CancellationToken cancellationToken
  )
  {
    var draft = await _draftRepository.GetByPublicIdAsync(
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

    if (draft.GuestDraftStatus != DraftStatus.Created)
    {
      return Result.Failure(InvitationErrors.DraftAlreadyStarted);
    }

    var invitation = await _invitationRepository.GetByPublicIdAsync(
      request.InvitationPublicId,
      cancellationToken
    );

    if (invitation is null || invitation.DraftId != draft.Id)
    {
      return Result.Failure(InvitationErrors.NotFound(request.InvitationPublicId));
    }

    var result = invitation.Resend(draft.PublicId, _dateTimeProvider.UtcNow);

    if (result.IsFailure)
    {
      return result;
    }

    _invitationRepository.Update(invitation);

    return Result.Success();
  }
}
