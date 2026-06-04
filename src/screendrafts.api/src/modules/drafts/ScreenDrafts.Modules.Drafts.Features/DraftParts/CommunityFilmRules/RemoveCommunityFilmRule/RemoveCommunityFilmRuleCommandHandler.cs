namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.RemoveCommunityFilmRule;

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

    var result = draftPart.RemoveCommunityFilmRule(request.RuleId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
