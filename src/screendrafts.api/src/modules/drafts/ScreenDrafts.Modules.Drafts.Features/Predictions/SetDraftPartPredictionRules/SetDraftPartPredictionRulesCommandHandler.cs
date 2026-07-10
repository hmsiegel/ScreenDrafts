namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictionRules;

internal sealed class SetDraftPartPredictionRulesCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IPublicIdGenerator publicIdGenerator,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<SetDraftPartPredictionRulesCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    SetDraftPartPredictionRulesCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartPublicId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure<string>(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var existing = await _rulesRepository.GetByDraftPartIdAsync(draftPart.Id, cancellationToken);

    var mode = PredictionMode.FromValue(request.PredictionMode);

    // Idempotent-replace, matching the "Set" naming convention used elsewhere
    // in this codebase (SetDraftPositions, SetDraftCategories) rather than
    // create-only. Admins re-save the whole draft-edit form on every change,
    // which resends this payload unconditionally even when only an unrelated
    // field (e.g. host assignment) changed.
    if (existing is not null)
    {
      var updateResult = existing.UpdateRules(
        predictionMode: mode,
        requiredCount: request.RequiredCount,
        topN: request.TopN,
        deadlineUtc: request.DeadlineUtc,
        updatedOnUtc: _dateTimeProvider.UtcNow
      );

      if (updateResult.IsFailure)
      {
        return updateResult;
      }

      _rulesRepository.Update(existing);

      return Result.Success();
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPartPredictionRules);

    var rulesResult = DraftPartPredictionRule.Create(
      publicId: publicId,
      draftPart: draftPart,
      predictionMode: mode,
      requiredCount: request.RequiredCount,
      topN: request.TopN,
      deadlineUtc: request.DeadlineUtc
    );

    if (rulesResult.IsFailure)
    {
      return Result.Failure(rulesResult.Errors);
    }

    var rules = rulesResult.Value;

    _rulesRepository.Add(rules);

    return Result.Success();
  }
}
