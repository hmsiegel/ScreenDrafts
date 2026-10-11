namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Accept;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class AcceptPendingInvitationsCommandHandler(
  IInvitationRepository invitationRepository,
  IDraftRepository draftRepository,
  IDrafterRepository drafterRepository,
  IPublicIdGenerator publicIdGenerator,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<AcceptPendingInvitationsCommand>
{
  private readonly IInvitationRepository _invitationRepository = invitationRepository;
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IDrafterRepository _drafterRepository = drafterRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    AcceptPendingInvitationsCommand request,
    CancellationToken cancellationToken
  )
  {
    var email = Invitation.NormalizeEmail(request.Email);

    var invitations = await _invitationRepository.GetPendingByEmailAsync(email, cancellationToken);

    if (invitations.Count == 0)
    {
      return Result.Success();
    }

    // The CreateDrafter consumer may not have run yet. Create the Drafter here if missing.
    var drafter = await _drafterRepository.GetByUserIdAsync(request.UserId, cancellationToken);

    if (drafter is null)
    {
      var createResult = Drafter.Create(
        _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDrafter),
        request.UserId,
        request.FirstName,
        request.LastName
      );

      if (createResult.IsFailure)
      {
        return Result.Failure(createResult.Errors);
      }

      drafter = createResult.Value;
      _drafterRepository.Add(drafter);
    }

    var now = _dateTimeProvider.UtcNow;

    foreach (var invitation in invitations)
    {
      var draft = await _draftRepository.GetByIdAsync(invitation.DraftId, cancellationToken);

      // Draft gone or already started: the invite stays pending.
      if (draft is null || draft.GuestDraftStatus != DraftStatus.Created)
      {
        continue;
      }

      var participant = Participant.From(drafter.Id);

      if (draft.FindByParticipantRef(participant) is null)
      {
        var isOwner = request.UserId == draft.OwnerUserId;
        var addResult = draft.AddParticipant(participant, isOwner);

        if (addResult.IsFailure)
        {
          continue;
        }

        _draftRepository.Update(draft);
      }

      var acceptResult = invitation.Accept(request.UserId, now);

      if (acceptResult.IsSuccess)
      {
        _invitationRepository.Update(invitation);
      }
    }

    return Result.Success();
  }
}
