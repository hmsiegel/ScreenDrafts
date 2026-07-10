namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SetDraftPartPredictors;

internal sealed class SetDraftPartPredictorsCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDraftPartPredictorRepository predictorRepository,
  IPredictionContestantRepository contestantRepository,
  IPersonRepository personRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<SetDraftPartPredictorsCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDraftPartPredictorRepository _predictorRepository = predictorRepository;
  private readonly IPredictionContestantRepository _contestantRepository = contestantRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SetDraftPartPredictorsCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartPublicId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    if (
      request.Predictors.Select(p => p.ContestantPublicId).Distinct().Count()
      != request.Predictors.Count
    )
    {
      return Result.Failure(PredictionErrors.DuplicateContestantInPredictorList);
    }

    var resolved = new List<(PredictionContestant Contestant, PersonId? Submitter)>();

    foreach (var entry in request.Predictors)
    {
      var contestant = await _contestantRepository.GetByPublicIdAsync(
        entry.ContestantPublicId,
        cancellationToken
      );

      if (contestant is null)
      {
        return Result.Failure(PredictionErrors.ContestantNotFound(entry.ContestantPublicId));
      }

      PersonId? submitterId = null;

      if (entry.AllowedSubmitterPersonPublicId is not null)
      {
        var submitter = await _personRepository.GetByPublicIdAsync(
          entry.AllowedSubmitterPersonPublicId,
          cancellationToken
        );

        if (submitter is null)
        {
          return Result.Failure(
            PredictionErrors.SubmitterPersonNotFound(entry.AllowedSubmitterPersonPublicId)
          );
        }

        submitterId = submitter.Id;
      }

      resolved.Add((contestant, submitterId));
    }

    var existing = await _predictorRepository.GetByDraftPartIdAsync(
      draftPart.Id,
      cancellationToken
    );

    var resolvedContestantIds = resolved.Select(r => r.Contestant.Id).ToHashSet();

    var toRemove = existing.Where(e => !resolvedContestantIds.Contains(e.ContestantId)).ToList();
    if (toRemove.Count > 0)
    {
      _predictorRepository.RemoveRange(toRemove);
    }

    foreach (var (contestant, submitterId) in resolved)
    {
      var match = existing.FirstOrDefault(e => e.ContestantId == contestant.Id);

      if (match is not null)
      {
        match.SetAllowedSubmitter(submitterId);
        _predictorRepository.Update(match);
        continue;
      }

      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPartPredictor);
      var predictor = DraftPartPredictor.Create(
        publicId: publicId,
        draftPartId: draftPart.Id,
        contestant: contestant,
        allowedSubmitterPersonId: submitterId
      );

      _predictorRepository.Add(predictor);
    }

    return Result.Success();
  }
}
