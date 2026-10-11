namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Invite;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class InviteGuestDrafterCommandHandler(
  IDraftRepository draftRepository,
  IInvitationRepository invitationRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<InviteGuestDrafterCommand, InviteGuestDrafterResponse>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IInvitationRepository _invitationRepository = invitationRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<InviteGuestDrafterResponse>> Handle(
    InviteGuestDrafterCommand request,
    CancellationToken cancellationToken
  )
  {
    var draft = await _draftRepository.GetByPublicIdAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (draft is null)
    {
      return Result.Failure<InviteGuestDrafterResponse>(
        DraftErrors.NotFound(request.GuestDraftPublicId)
      );
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure<InviteGuestDrafterResponse>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId)
      );
    }

    if (caller.UserId != draft.OwnerUserId)
    {
      return Result.Failure<InviteGuestDrafterResponse>(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    if (draft.GuestDraftStatus != DraftStatus.Created)
    {
      return Result.Failure<InviteGuestDrafterResponse>(InvitationErrors.DraftAlreadyStarted);
    }

    var email = Invitation.NormalizeEmail(request.Email);

    // IUsersApi has no get-by-email. Search matches names and email, so filter exactly.
    var matches = await _usersApi.GetAllUsersAsync(email, cancellationToken);

    if (matches.Any(u => string.Equals(u.Email?.Trim(), email, StringComparison.OrdinalIgnoreCase)))
    {
      return Result.Failure<InviteGuestDrafterResponse>(InvitationErrors.EmailAlreadyRegistered);
    }

    var draftId = draft.Id.Value;

    var existing = await _invitationRepository.GetByDraftAndEmailAsync(
      draftId,
      email,
      cancellationToken
    );

    if (existing is not null)
    {
      return Result.Failure<InviteGuestDrafterResponse>(InvitationErrors.AlreadyInvited);
    }

    var count = await _invitationRepository.CountForDraftAsync(draftId, cancellationToken);

    if (count >= Invitation.MaxPerDraft)
    {
      return Result.Failure<InviteGuestDrafterResponse>(InvitationErrors.TooManyInvitations);
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraftInvitation);

    var createResult = Invitation.Create(
      publicId: publicId,
      draftId: draft.Id,
      draftPublicId: draft.PublicId,
      email: email,
      name: request.Name,
      invitedByUserId: caller.UserId,
      utcNow: _dateTimeProvider.UtcNow
    );

    if (createResult.IsFailure)
    {
      return Result.Failure<InviteGuestDrafterResponse>(createResult.Errors);
    }

    _invitationRepository.Add(createResult.Value);

    return Result.Success(new InviteGuestDrafterResponse { PublicId = publicId });
  }
}
