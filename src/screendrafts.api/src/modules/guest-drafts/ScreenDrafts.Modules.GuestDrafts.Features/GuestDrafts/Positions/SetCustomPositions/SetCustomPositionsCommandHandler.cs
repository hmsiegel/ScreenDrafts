namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;

internal sealed class SetCustomPositionsCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<SetCustomPositionsCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SetCustomPositionsCommand request,
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

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, ct);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    if (caller.UserId != guestDraft.OwnerUserId)
    {
      return Result.Failure(GuestDraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var positions = request
      .Positions.Select(p =>
        (p.Name, p.Picks, p.HasBonusVeto, p.HasBonusVetoOverride, p.HasBonusFungibleToken)
      )
      .ToList();

    var result = guestDraft.SetCustomPositions(
      positions,
      _ => _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraftPosition)
    );

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}
