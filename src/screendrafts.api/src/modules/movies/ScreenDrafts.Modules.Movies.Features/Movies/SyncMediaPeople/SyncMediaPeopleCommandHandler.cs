namespace ScreenDrafts.Modules.Movies.Features.Movies.SyncMediaPeople;

internal sealed partial class SyncMediaPeopleCommandHandler(
  IMediaRepository mediaRepository,
  MediaPeopleAttacher peopleAttacher,
  ILogger<SyncMediaPeopleCommandHandler> logger
) : ICommandHandler<SyncMediaPeopleCommand>
{
  private readonly IMediaRepository _mediaRepository = mediaRepository;
  private readonly MediaPeopleAttacher _peopleAttacher = peopleAttacher;
  private readonly ILogger<SyncMediaPeopleCommandHandler> _logger = logger;

  public async Task<Result> Handle(
    SyncMediaPeopleCommand request,
    CancellationToken cancellationToken
  )
  {
    var media =
      request.MediaType == MediaType.TvEpisode
        ? await _mediaRepository.FindByTvEpisodeForUpdateAsync(
          request.TvSeriesTmdbId!.Value,
          request.SeasonNumber!.Value,
          request.EpisodeNumber!.Value,
          cancellationToken
        )
        : await _mediaRepository.FindByTmdbIdForUpdateAsync(
          request.TmdbId,
          request.MediaType,
          cancellationToken
        );

    if (media is null)
    {
      LogMediaNotFound(_logger, request.TmdbId, request.MediaType.Name);
      return Result.Failure(MediaErrors.MediaNotFound(request.TmdbId));
    }

    await _peopleAttacher.AttachAsync(
      media,
      request.Directors,
      request.Actors,
      request.Writers,
      request.Producers,
      request.ProductionCompanies,
      cancellationToken
    );

    return Result.Success();
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Warning,
    Message = "SyncMediaPeople: no media found for TmdbId {TmdbId} ({MediaType})."
  )]
  private static partial void LogMediaNotFound(
    ILogger<SyncMediaPeopleCommandHandler> logger,
    int tmdbId,
    string mediaType
  );
}
