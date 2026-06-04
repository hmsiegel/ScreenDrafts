namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AssignFilmToCommunityFilmRule;

internal sealed class AssignFilmToCommunityFilmRuleCommandHandler(
  IDraftPartRepository draftPartRepository
) : ICommandHandler<AssignFilmToCommunityFilmRuleCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(
    AssignFilmToCommunityFilmRuleCommand request,
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

    var result = draftPart.AssignFilmToCommunityFilmRule(request.RuleId, request.TmdbId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
