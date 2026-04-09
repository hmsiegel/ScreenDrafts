namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictionRules;

internal sealed class SetDraftPartPredictionRulesCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<SetDraftPartPredictionRulesCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SetDraftPartPredictionRulesCommand request,
    CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartPublicId,
      cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<string>(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var existing = await _rulesRepository.GetByDraftPartIdAsync(
      draftPart.Id,
      cancellationToken);

    if (existing is not null)
    {
      return Result.Failure<string>(
        PredictionErrors.RulesAlreadyExist(request.DraftPartPublicId));
    }

    var mode = PredictionMode.FromValue(request.PredictionMode);

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPartPredictionRules);

    var rulesResult = DraftPartPredictionRule.Create(
      publicId: publicId,
      draftPart: draftPart,
      predictionMode: mode,
      requiredCount: request.RequiredCount,
      topN: request.TopN,
      deadlineUtc: request.DeadlineUtc);

    if (rulesResult.IsFailure)
    {
      return Result.Failure(rulesResult.Errors);
    }

    var rules = rulesResult.Value;

    _rulesRepository.Add(rules);

    return Result.Success();
  }
}
