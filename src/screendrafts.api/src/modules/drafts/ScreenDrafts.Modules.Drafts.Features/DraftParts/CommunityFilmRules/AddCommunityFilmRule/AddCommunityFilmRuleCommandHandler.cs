namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AddCommunityFilmRule;

internal sealed class AddCommunityFilmRuleCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<AddCommunityFilmRuleCommand, string>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    AddCommunityFilmRuleCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure<string>(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.CommunityFilmRule);

    var ruleKind = CommunityFilmRuleKind.FromValue(request.RuleKind);

    var result = draftPart.AddCommunityFilmRule(publicId, ruleKind, request.TargetSlot);

    var communityPositionResult = draftPart.EnsureCommunityPositions(() =>
      _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPosition)
    );

    if (communityPositionResult.IsFailure)
    {
      return Result.Failure<string>(communityPositionResult.Errors);
    }

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success(publicId);
  }
}
