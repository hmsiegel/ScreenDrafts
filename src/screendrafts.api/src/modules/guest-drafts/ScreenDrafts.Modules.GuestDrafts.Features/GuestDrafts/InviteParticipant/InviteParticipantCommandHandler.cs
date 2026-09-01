namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.InviteParticipant;

internal sealed class InviteParticipantCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<InviteParticipantCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    InviteParticipantCommand request,
    CancellationToken cancellationToken
  )
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdWithParticipantsAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (guestDraft is null)
    {
      return Result.Failure(GuestDraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var user = await _usersApi.GetUserByPublicId(request.UserPublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.UserPublicId));
    }

    var participantPublicId = _publicIdGenerator.GeneratePublicId(
      PublicIdPrefixes.GuestDraftParticipant
    );

    var result = guestDraft.InviteParticipant(participantPublicId, user.UserId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Error!);
    }

    _guestDraftRepository.Update(guestDraft);

    return Result.Success();
  }
}
