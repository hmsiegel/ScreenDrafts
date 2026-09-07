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
    CancellationToken cancellationToken
  )
  {
    var owner = await _usersApi.GetUserByPublicId(request.OwnerUserPublicId, cancellationToken);

    if (owner is null)
    {
      return Result.Failure<string>(
        UserPublicApiErrors.PublicIdNotFound(request.OwnerUserPublicId)
      );
    }

    if (!GuestDraftType.TryFromName(request.Type, ignoreCase: true, out var type))
    {
      return Result.Failure<string>(GuestDraftErrors.InvalidType(request.Type));
    }

    if (request.NumberOfPicks < 1)
    {
      return Result.Failure<string>(GuestDraftErrors.NumberOfPicksMustBeGreaterThanZero);
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraft);

    var createResult = GuestDraft.Create(
      publicId: publicId,
      ownerUserId: owner.UserId,
      title: request.Title,
      guestDraftType: type,
      draftDate: request.DraftDate
    );

    if (createResult.IsFailure)
    {
      return Result.Failure<string>(createResult.Errors);
    }

    var guestDraft = createResult.Value;

    Result boardResult;

    if (GuestDraftBoardTemplates.IsFixed(type))
    {
      boardResult = guestDraft.UseFixedBoardLayout(_ =>
        _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraftPosition)
      );
    }
    else
    {
      var coverageResult = GuestDraftPositionCoverage.Validate(
        [.. request.Positions.Select(p => p.Picks)],
        request.NumberOfPicks
      );

      if (coverageResult.IsFailure)
      {
        return Result.Failure<string>(coverageResult.Errors);
      }

      var positions = request
        .Positions.Select(p =>
          (p.Name, p.Picks, p.HasBonusVeto, p.HasBonusVetoOverride, p.HasBonusFungibleToken)
        )
        .ToList();

      boardResult = guestDraft.SetCustomPositions(
        positions,
        _ => _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraftPosition)
      );
    }

    if (boardResult.IsFailure)
    {
      return Result.Failure<string>(boardResult.Errors);
    }

    _guestDraftRepository.Add(guestDraft);

    return Result.Success(guestDraft.PublicId);
  }
}
