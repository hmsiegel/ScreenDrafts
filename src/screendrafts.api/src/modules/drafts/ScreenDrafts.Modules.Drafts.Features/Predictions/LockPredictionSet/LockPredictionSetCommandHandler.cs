namespace ScreenDrafts.Modules.Drafts.Features.Predictions.LockPredictionSet;

internal sealed class LockPredictionSetCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IDraftPredictionSetRepository setRepository,
  IDateTimeProvider dateTimeProvider)
  : ICommandHandler<LockPredictionSetCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IDraftPredictionSetRepository _setRepository = setRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(
    LockPredictionSetCommand request,
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

    var set = await _setRepository.GetByPublicIdAsync(request.SetPublicId, cancellationToken);

    if (set is null)
    {
      return Result.Failure(PredictionErrors.SetNotFound(request.SetPublicId));
    }

    var snapshot = new PredictionRulesSnapshot(
      Mode: rules.PredictionMode,
      RequiredCount: rules.RequiredCount,
      TopN: rules.TopN);

    var result = set.Lock(
      rulesSnapshot: snapshot,
      now: _dateTimeProvider.UtcNow);

    if (result.IsFailure)
    {
      return result;
    }

    _setRepository.Update(set);

    return Result.Success();
  }
}
