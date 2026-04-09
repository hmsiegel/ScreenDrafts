namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed class SubmitPredictionSetCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPredictionSeasonRepository seasonRepository,
  IPredictionContestantRepository contestantRepository,
  IPersonRepository personRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IDraftPredictionSetRepository setRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<SubmitPredictionSetCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPredictionSeasonRepository _seasonRepository = seasonRepository;
  private readonly IPredictionContestantRepository _contestantRepository = contestantRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IDraftPredictionSetRepository _setRepository = setRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SubmitPredictionSetCommand request,
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

    if (rules.DeadlineUtc.HasValue && DateTime.UtcNow > rules.DeadlineUtc.Value)
    {
      return Result.Failure(PredictionErrors.DeadlinePassed);
    }

    if (request.Entries.Count != rules.RequiredCount)
    {
      return Result.Failure(
        PredictionErrors.InvalidEntryCount(rules.RequiredCount, request.Entries.Count));
    }

    var season = await _seasonRepository.GetByPublicIdAsync(
      request.SeasonPublicId,
      cancellationToken);

    if (season is null)
    {
      return Result.Failure(PredictionErrors.SeasonNotFound(request.SeasonPublicId));
    }

    var contestant = await _contestantRepository.GetByPublicIdAsync(
      request.ContestantPublicId,
      cancellationToken);

    if (contestant is null)
    {
      return Result.Failure(PredictionErrors.ContestantNotFound(request.ContestantPublicId));
    }

    var existing = await _setRepository.GetByContestantAndDraftPartAsync(
      contestant.Id,
      draftPart.Id,
      cancellationToken);

    if (existing is not null)
    {
      return Result.Failure(PredictionErrors.SetAlreadyExists);
    }

    Person? submittedByPerson = null;
    if (request.SubmittedByPersonPublicId is not null)
    {
      submittedByPerson = await _personRepository.GetByPublicIdAsync(
        request.SubmittedByPersonPublicId,
        cancellationToken);
    }

    var sourceKind = PredictionSourceKind.FromValue(request.SourceKind);

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPredictionSet);

    var setResult = DraftPredictionSet.Create(
      publicId: publicId,
      season: season,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: submittedByPerson,
      sourceKind: sourceKind);

    if (setResult.IsFailure)
    {
      return Result.Failure(setResult.Errors);
    }

    var set = setResult.Value;

    var entries = request.Entries.Select(dto =>
      PredictionEntry.Create(
        predictionSet: set,
        mediaPublicId: dto.MediaPublicId,
        mediaTitle: dto.MediaTitle,
        orderIndex: dto.OrderIndex,
        notes: dto.Notes)).ToList();

    var replaceResult = set.ReplaceEntries(entries);

    if (replaceResult.IsFailure)
    {
      return replaceResult;
    }

    _setRepository.Add(set);

    return Result.Success();
  }
}
