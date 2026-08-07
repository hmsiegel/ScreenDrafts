namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedSubmitPredictionsSet;

internal sealed class SeedSubmitPredictionSetCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPredictionSeasonRepository seasonRepository,
  IPredictionContestantRepository contestantRepository,
  IPersonRepository personRepository,
  IDraftPartPredictionRulesRepository rulesRepository,
  IDraftPredictionSetRepository setRepository,
  IDraftPartPredictorRepository predictorRepository,
  IPublicIdGenerator publicIdGenerator,
  IMovieRepository movieRepository,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<SeedSubmitPredictionSetCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPredictionSeasonRepository _seasonRepository = seasonRepository;
  private readonly IPredictionContestantRepository _contestantRepository = contestantRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IDraftPartPredictionRulesRepository _rulesRepository = rulesRepository;
  private readonly IDraftPredictionSetRepository _setRepository = setRepository;
  private readonly IDraftPartPredictorRepository _predictorRepository = predictorRepository;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SeedSubmitPredictionSetCommand request,
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

    // Same race-closing check as the live handler: a set can only land
    // before the part starts, since Start() locks every existing set.
    if (draftPart.Status != DraftPartStatus.Created)
    {
      return Result.Failure(PredictionErrors.DraftPartAlreadyStarted);
    }

    var contestant = await _contestantRepository.GetByPublicIdAsync(
      request.ContestantPublicId,
      cancellationToken
    );

    if (contestant is null)
    {
      return Result.Failure(PredictionErrors.ContestantNotFound(request.ContestantPublicId));
    }

    // Existence check only — confirms this contestant is actually
    // configured to predict on this draft part. No authorization
    // comparison against AllowedSubmitterPersonId; that's the live
    // handler's job, not this one's.
    var predictor = await _predictorRepository.GetByDraftPartAndContestantAsync(
      draftPart.Id,
      contestant.Id,
      cancellationToken
    );

    if (predictor is null)
    {
      return Result.Failure(PredictionErrors.PredictorNotConfigured(request.ContestantPublicId));
    }

    var rules = await _rulesRepository.GetByDraftPartIdAsync(draftPart.Id, cancellationToken);

    if (rules is null)
    {
      return Result.Failure(PredictionErrors.RulesNotFound(request.DraftPartPublicId));
    }

    if (request.Entries.Count < 1 || request.Entries.Count > rules.RequiredCount)
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

    // Sequential for the same reason as the live handler — one scoped
    // DbContext underneath these repositories, so concurrent calls on it
    // throw.
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

    if (existing is not null)
    {
      var replaceEntries = request
        .Entries.Select(dto =>
          PredictionEntry.Create(
            predictionSet: existing,
            tmdbId: dto.TmdbId,
            mediaTitle: dto.MediaTitle,
            orderIndex: dto.OrderIndex,
            notes: dto.Notes
          )
        )
        .ToList();

      var replaceExistingResult = existing.ReplaceEntries(replaceEntries);

      if (replaceExistingResult.IsFailure)
      {
        return replaceExistingResult;
      }

      _setRepository.Update(existing);

      return Result.Success();
    }

    Person? submittedByPerson = null;

    if (request.SubmittedByPersonPublicId is not null)
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
