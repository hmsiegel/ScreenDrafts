namespace ScreenDrafts.Modules.Drafts.Features.Predictions.ScoreDraftPartPredictions;

internal sealed class ScoreDraftPartPredictionsCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IDraftPredictionSetRepository setRepository,
  IPredictionResultRepository resultRepository,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<ScoreDraftPartPredictionsCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IDraftPredictionSetRepository _setRepository = setRepository;
  private readonly IPredictionResultRepository _resultRepository = resultRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    ScoreDraftPartPredictionsCommand request,
    CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartPublicId,
      cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var rules = await _rulesRepository.GetByDraftPartIdAsync(draftPart.Id, cancellationToken);

    if (rules is null)
    {
      return Result.Failure(PredictionErrors.RulesNotFound(request.DraftPartPublicId));
    }

    var sets = await _setRepository.GetByDraftPartIdAsync(draftPart.Id, cancellationToken);

    if (sets.Count == 0)
    {
      return Result.Success();
    }

    var existingResults = await _resultRepository.GetByDraftPartIdAsync(
      draftPart.Id,
      cancellationToken);

    if (existingResults.Count > 0)
    {
      return Result.Failure(PredictionErrors.AlreadyScored(request.DraftPartPublicId));
    }

    var now = _dateTimeProvider.UtcNow;
    var resultsBySetId = new Dictionary<DraftPredictionSetId, PredictionResult>();

    foreach (var set in sets)
    {
      if (!set.IsLocked)
      {
        var snapshot = new PredictionRulesSnapshot(
          rules.PredictionMode,
          rules.RequiredCount,
          rules.TopN);

        var lockResult = set.Lock(snapshot, now);
        if (lockResult.IsFailure)
        {
          return lockResult;
        }
      }

      var predictionResult = PredictionScoringService.Score(
        set: set,
        finalMediaPublicIds: request.FinalMediaPublicIds,
        rules: rules,
        scoredAtUtc: now);

      if (predictionResult.IsFailure)
      {
        return Result.Failure(predictionResult.Errors);
      }

      var prediction = predictionResult.Value;

      resultsBySetId[set.Id] = prediction;
      _resultRepository.Add(prediction);
    }

    var surrogateSetIds = sets
      .SelectMany(s => s.Surrogates)
      .Select(sa => sa.SurrogateSetId)
      .ToHashSet();

    foreach (var set in sets)
    {
      if (surrogateSetIds.Contains(set.Id))
      {
        continue;
      }

      var primaryResult = resultsBySetId[set.Id];
      int finalPoints;

      if (set.Surrogates.Count > 0)
      {
        finalPoints = primaryResult.PointsAwarded;

        foreach (var assignment in set.Surrogates)
        {
          if (!resultsBySetId.TryGetValue(assignment.SurrogateSetId, out var surrogateResult))
          {
            continue;
          }

          var resolved = SurrogateScoreResolver.Resolve(
            assignment,
            primaryResult,
            surrogateResult);

          finalPoints = Math.Max(finalPoints, resolved);
        }
      }
      else
      {
        finalPoints = primaryResult.PointsAwarded;
      }

      set.RaiseScored(
        pointsAwarded: finalPoints,
        shootTheMoon: primaryResult.ShootTheMoon,
        seasonId: set.SeasonId);

      _setRepository.Update(set);
    }

    return Result.Success();
  }
}
