using CsvParser = ScreenDrafts.Modules.Drafts.Features.Common.BulkAdd.CsvParser;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.BulkAddMoviesToDraftBoard;

internal sealed record BulkAddMoviesToDraftBoardCommandHandler(
    IDraftBoardRepository draftBoardRepository,
    IDraftRepository draftRepository,
    IMovieRepository movieRepository,
    IEventBus eventBus,
    IPublicIdGenerator publicIdGenerator,
    DraftBoardParticipantResolver draftBoardParticipantResolver,
    ICacheService cacheService,
    IDateTimeProvider dateTimeProvider)
  : ICommandHandler<BulkAddMoviesToDraftBoardCommand, BulkAddMoviesResponse>
{
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IMovieRepository _movieRepository = movieRepository;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly DraftBoardParticipantResolver _draftBoardParticipantResolver = draftBoardParticipantResolver;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<BulkAddMoviesResponse>> Handle(BulkAddMoviesToDraftBoardCommand request, CancellationToken cancellationToken)
  {
    var resolved = await _draftBoardParticipantResolver.ResolveAsync(request.UserPublicId, cancellationToken);

    if (resolved is null)
    {
      return Result.Failure<BulkAddMoviesResponse>(DraftBoardErrors.ParticipantNotFound(request.UserPublicId));
    }

    var draft = await _draftRepository.GetByPublicIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<BulkAddMoviesResponse>(DraftErrors.NotFound(request.DraftId));
    }

    if (draft.HasPool)
    {
      return Result.Failure<BulkAddMoviesResponse>(DraftPoolErrors.DraftHasPool);
    }

    var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
      draftId: draft.Id,
      participantId: resolved.Participant,
      cancellationToken: cancellationToken);

    if (board is null)
    {
      var boardResult = DraftBoard.Create(
        draftId: draft.Id,
        participant: resolved.Participant,
        publicId: _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftBoard));

      if (boardResult.IsFailure)
      {
        return Result.Failure<BulkAddMoviesResponse>(boardResult.Errors);
      }

      board = boardResult.Value;
      _draftBoardRepository.Add(board);
    }

    var rows = CsvParser.Parse(request.CsvStream);

    var validTmdbIds = rows
      .Where(r => r.TmdbId.HasValue)
      .Select(r => r.TmdbId!.Value)
      .Distinct()
      .ToList();

    var existingInDb = await _movieRepository.GetExistingTmdbIdsAsync(validTmdbIds, cancellationToken);

    var added = 0;
    var skipped = 0;
    var failed = 0;
    var failures = new List<BulkAddFailureDetail>();
    var tmdbIdsToFetch = new List<int>();

    foreach (var row in rows)
    {
      if (!row.TmdbId.HasValue)
      {
        failed++;
        failures.Add(new BulkAddFailureDetail
        {
          RowNumber = row.RowNumber,
          Title = row.Title,
          Reason = "Missing TMDb ID"
        });
        continue;
      }

      var addResult = board.AddItem(
        tmdbId: row.TmdbId.Value,
        notes: null,
        priority: null);

      if (addResult.IsFailure)
      {
        if (addResult.Errors.Any(e => e.Code == DraftBoardErrors.MovieAlreadyOnTheBoard(row.TmdbId.Value).Code))
        {
          skipped++;
          continue;
        }

        failed++;
        failures.Add(new BulkAddFailureDetail
        {
          RowNumber = row.RowNumber,
          Title = row.Title,
          Reason = string.Join(", ", addResult.Errors.Select(e => e.Description))
        });
        continue;
      }

      if (!existingInDb.Contains(row.TmdbId.Value))
      {
        tmdbIdsToFetch.Add(row.TmdbId.Value);
      }

      added++;
    }

    await _cacheService.RemoveAsync
      (
      key: DraftsCacheKeys.DraftBoard(
        draftPublicId: request.DraftId,
        userId: resolved.UserId),
      cancellationToken: cancellationToken);

    foreach (var tmdbId in tmdbIdsToFetch.Distinct())
    {
      await _eventBus.PublishAsync(
        new FetchMovieRequestedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: _dateTimeProvider.UtcNow,
          tmdbId: tmdbId),
        cancellationToken: cancellationToken);
    }

    _draftBoardRepository.Update(board);

    return Result.Success(new BulkAddMoviesResponse
    {
      TotalRows = rows.Count,
      AddedEntries = added,
      SkipedEntries = skipped,
      FailedEntries = failed,
      Failures = failures
    });
  }
}
