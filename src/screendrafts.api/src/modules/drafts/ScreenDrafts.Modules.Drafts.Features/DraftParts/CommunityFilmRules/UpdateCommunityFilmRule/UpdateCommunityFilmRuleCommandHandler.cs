namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.UpdateCommunityFilmRule;

internal sealed class RemoveCommunityFilmRuleCommandHandler(
  IDraftPartRepository draftPartRepository
) : ICommandHandler<RemoveCommunityFilmRuleCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(
    RemoveCommunityFilmRuleCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var ruleKind = CommunityFilmRuleKind.FromValue(request.RuleKind);

    var result = draftPart.UpdateCommunityFilmRule(request.RuleId, ruleKind, request.TargetSlot);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
