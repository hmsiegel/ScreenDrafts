namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Create;

internal sealed class CreateDraftCommandHandler(
  IDraftRepository draftRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreateDraftCommand, string>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    CreateDraftCommand request,
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

    if (!DraftType.TryFromName(request.Type, ignoreCase: true, out var type))
    {
      return Result.Failure<string>(DraftErrors.InvalidType(request.Type));
    }

    if (request.NumberOfPicks < 1)
    {
      return Result.Failure<string>(DraftErrors.NumberOfPicksMustBeGreaterThanZero);
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraft);

    var createResult = Draft.Create(
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

    if (GameBoardTemplates.IsFixed(type))
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

    _draftRepository.Add(guestDraft);

    return Result.Success(guestDraft.PublicId);
  }
}
