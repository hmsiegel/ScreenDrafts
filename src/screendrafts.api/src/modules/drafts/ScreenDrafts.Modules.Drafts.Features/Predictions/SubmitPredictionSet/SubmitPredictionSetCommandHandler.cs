namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed class SubmitPredictionSetCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPredictionSeasonRepository seasonRepository,
  IPredictionContestantRepository contestantRepository,
  IPersonRepository personRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IDraftPredictionSetRepository setRepository,
  IDraftPartPredictorRepository predictorRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator,
  IMovieRepository movieRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<SubmitPredictionSetCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPredictionSeasonRepository _seasonRepository = seasonRepository;
  private readonly IPredictionContestantRepository _contestantRepository = contestantRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IDraftPredictionSetRepository _setRepository = setRepository;
  private readonly IDraftPartPredictorRepository _predictorRepository = predictorRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SubmitPredictionSetCommand request,
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

    var contestant = await _contestantRepository.GetByPublicIdAsync(
      request.ContestantPublicId,
      cancellationToken
    );

    if (contestant is null)
    {
      return Result.Failure(PredictionErrors.ContestantNotFound(request.ContestantPublicId));
    }

    // ── Authorization: caller must be the contestant, or the designated
    // surrogate for this contestant on this draft part. ──────────────────────
    var callerUserId = await _usersApi.GetUserByPublicId(
      request.ActorUserPublicId,
      cancellationToken
    );

    if (callerUserId is null)
    {
      return Result.Failure(PredictionErrors.NotAuthorizedToSubmit);
    }

    var callerPerson = await _personRepository.GetByUserIdAsync(
      callerUserId.UserId,
      cancellationToken
    );

    if (callerPerson is null)
    {
      return Result.Failure(PredictionErrors.NotAuthorizedToSubmit);
    }

    var predictor = await _predictorRepository.GetByDraftPartAndContestantAsync(
      draftPart.Id,
      contestant.Id,
      cancellationToken
    );

    if (predictor is null)
    {
      return Result.Failure(PredictionErrors.PredictorNotConfigured(request.ContestantPublicId));
    }

    var callerIsContestant = contestant.PersonId == callerPerson.Id;
    var callerIsDesignatedSubmitter = predictor.AllowedSubmitterPersonId == callerPerson.Id;

    if (!callerIsContestant && !callerIsDesignatedSubmitter)
    {
      return Result.Failure(PredictionErrors.NotAuthorizedToSubmit);
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
        PredictionErrors.InvalidEntryCount(rules.RequiredCount, request.Entries.Count)
      );
    }

    if (request.Entries.Select(e => e.TmdbId).Distinct().Count() != request.Entries.Count)
    {
      return Result.Failure(PredictionErrors.DuplicateEntryInSet);
    }

    var season = await _seasonRepository.GetByPublicIdAsync(
      request.SeasonPublicId,
      cancellationToken
    );

    if (season is null)
    {
      return Result.Failure(PredictionErrors.SeasonNotFound(request.SeasonPublicId));
    }

    var existing = await _setRepository.GetByContestantAndDraftPartAsync(
      contestant.Id,
      draftPart.Id,
      cancellationToken
    );

    if (existing is not null)
    {
      return Result.Failure(PredictionErrors.SetAlreadyExists);
    }

    // Sequential — one scoped DbContext underneath these repositories.
    // Task.WhenAll over this loop fires concurrent operations on the same
    // context and throws; each check must finish before the next starts.
    foreach (var tmdbId in request.Entries.Select(entry => entry.TmdbId))
    {
      var existsInDb = await _movieRepository.ExistsByTmdbIdAsync(tmdbId, cancellationToken);

      if (!existsInDb)
      {
        await _eventBus.PublishAsync(
          new FetchMediaRequestedIntegrationEvent(
            id: Guid.NewGuid(),
            occurredOnUtc: _dateTimeProvider.UtcNow,
            tmdbId: tmdbId,
            igdbId: null,
            tvSeriesTmdbId: null,
            seasonNumber: null,
            episodeNumber: null,
            mediaType: MediaType.Movie,
            imdbId: null
          ),
          cancellationToken
        );
      }
    }

    // SubmittedByPersonPublicId, if given, is recorded for the audit trail —
    // authorization above already establishes the caller may act here.
    Person? submittedByPerson = callerIsDesignatedSubmitter ? callerPerson : null;
    if (submittedByPerson is null && request.SubmittedByPersonPublicId is not null)
    {
      submittedByPerson = await _personRepository.GetByPublicIdAsync(
        request.SubmittedByPersonPublicId,
        cancellationToken
      );
    }

    var sourceKind = PredictionSourceKind.FromValue(request.SourceKind);

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPredictionSet);

    var setResult = DraftPredictionSet.Create(
      publicId: publicId,
      season: season,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: submittedByPerson,
      sourceKind: sourceKind
    );

    if (setResult.IsFailure)
    {
      return Result.Failure(setResult.Errors);
    }

    var set = setResult.Value;

    var entries = request
      .Entries.Select(dto =>
        PredictionEntry.Create(
          predictionSet: set,
          tmdbId: dto.TmdbId,
          mediaTitle: dto.MediaTitle,
          orderIndex: dto.OrderIndex,
          notes: dto.Notes
        )
      )
      .ToList();

    var replaceResult = set.ReplaceEntries(entries);

    if (replaceResult.IsFailure)
    {
      return replaceResult;
    }

    _setRepository.Add(set);

    return Result.Success();
  }
}
