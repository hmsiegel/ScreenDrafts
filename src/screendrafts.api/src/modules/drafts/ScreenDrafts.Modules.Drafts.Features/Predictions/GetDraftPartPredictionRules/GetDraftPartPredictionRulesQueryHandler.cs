namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictionRules;

internal sealed class GetDraftPartPredictionRulesQueryHandler(
  IDraftPartRepository draftPartRepository,
  IDraftPartPredictionRulesRepository rulesRepository
) : IQueryHandler<GetDraftPartPredictionRulesQuery, GetDraftPartPredictionRulesResponse?>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;

  public async Task<Result<GetDraftPartPredictionRulesResponse?>> Handle(
    GetDraftPartPredictionRulesQuery request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartPublicId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure<GetDraftPartPredictionRulesResponse?>(
        DraftPartErrors.NotFound(request.DraftPartPublicId)
      );
    }

    var rules = await _rulesRepository.GetByDraftPartIdAsync(draftPart.Id, cancellationToken);

    if (rules is null)
    {
      return Result.Success<GetDraftPartPredictionRulesResponse?>(null);
    }

    return Result.Success<GetDraftPartPredictionRulesResponse?>(
      new GetDraftPartPredictionRulesResponse
      {
        PredictionMode = rules.PredictionMode.Value,
        RequiredCount = rules.RequiredCount,
        TopN = rules.TopN,
        DeadlineUtc = rules.DeadlineUtc,
      }
    );
  }
}
