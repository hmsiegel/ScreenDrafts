namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;

internal sealed class CreateGuestDraftCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreateGuestDraftCommand, string>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    CreateGuestDraftCommand request,
    CancellationToken cancellationToken)
  {
    var owner = await _usersApi.GetUserByPublicId(request.OwnerUserPublicId, cancellationToken);

    if (owner is null)
    {
      return Result.Failure<string>(UserPublicApiErrors.PublicIdNotFound(request.OwnerUserPublicId));
    }

    if (!GuestDraftType.TryFromName(request.Type, ignoreCase: true, out var type))
    {
      return Result.Failure<string>(GuestDraftErrors.InvalidType(request.Type));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraft);
    var ownerParticipantPublicId = _publicIdGenerator.GeneratePublicId(
      PublicIdPrefixes.GuestDraftParticipant);

    var result = GuestDraft.Create(
      publicId: publicId,
      ownerUserId: owner.UserId,
      ownerParticipantPublicId: ownerParticipantPublicId,
      title: request.Title,
      guestDraftType: type);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Error!);
    }

    _guestDraftRepository.Add(result.Value);

    return Result.Success(result.Value.PublicId);
  }
}
