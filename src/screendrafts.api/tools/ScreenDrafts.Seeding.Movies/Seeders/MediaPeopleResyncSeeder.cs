namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed partial class MediaPeopleResyncSeeder(
  MoviesDbContext dbContext,
  ISender sender,
  ILogger<MediaPeopleResyncSeeder> logger
) : ICustomSeeder
{
  private readonly MoviesDbContext _dbContext = dbContext;
  private readonly ISender _sender = sender;
  private readonly ILogger<MediaPeopleResyncSeeder> _logger = logger;

  public int Order => 10; // runs after MovieImdbSeeder (order 1) and MovieSeeder (order 2)
  public string Name => "mediapeopleresync";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMediaPeopleAsync(cancellationToken);
  }

  private async Task SeedMediaPeopleAsync(CancellationToken cancellationToken = default)
  {
    var incomplete = await _dbContext
      .Media.Where(m =>
        m.MediaType != MediaType.VideoGame
        && m.MediaType != MediaType.MusicVideo
        && !_dbContext.MediaDirectors.Any(md => md.MediaId == m.Id)
        && !_dbContext.MediaActors.Any(ma => ma.MediaId == m.Id)
        && !_dbContext.MediaProductionCompanies.Any(mpc => mpc.MediaId == m.Id)
      )
      .Select(m => new IncompleteMediaRecord(
        m.TmdbId,
        m.IgdbId,
        m.ImdbId,
        m.MediaType.Value,
        m.TvSeriesTmdbId,
        m.SeasonNumber,
        m.EpisodeNumber,
        m.PublicId
      ))
      .ToListAsync(cancellationToken);

    if (incomplete.Count == 0)
    {
      Log_NothingToResync(_logger);
      return;
    }

    Log_StartingResync(_logger, incomplete.Count);

    var succeeded = 0;
    var failed = 0;

    foreach (var item in incomplete)
    {
      try
      {
        var mediaType = MediaType.FromValue(item.MediaTypeValue);

        var fetchResult = await _sender.Send(
          new GetOnlineMediaCommand
          {
            MediaType = mediaType,
            TmdbId = item.TmdbId,
            IgdbId = item.IgdbId,
            ImdbId = item.ImdbId,
            TvSeriesTmdbId = item.TvSeriesTmdbId,
            SeasonNumber = item.SeasonNumber,
            EpisodeNumber = item.EpisodeNumber,
          },
          cancellationToken
        );

        if (fetchResult.IsFailure)
        {
          Log_FetchFailed(_logger, item.PublicId);
          failed++;
          continue;
        }

        var r = fetchResult.Value;

        var syncResult = await _sender.Send(
          new SyncMediaPeopleCommand
          {
            TmdbId = item.TmdbId!.Value,
            MediaType = mediaType,
            TvSeriesTmdbId = item.TvSeriesTmdbId,
            SeasonNumber = item.SeasonNumber,
            EpisodeNumber = item.EpisodeNumber,
            Directors = [.. r.Directors.Select(d => new PersonRequest(d.Name, d.ImdbId, d.TmdbId))],
            Actors = [.. r.Actors.Select(a => new PersonRequest(a.Name, a.ImdbId, a.TmdbId))],
            Writers = [.. r.Writers.Select(w => new PersonRequest(w.Name, w.ImdbId, w.TmdbId))],
            Producers = [.. r.Producers.Select(p => new PersonRequest(p.Name, p.ImdbId, p.TmdbId))],
            ProductionCompanies =
            [
              .. r.ProductionCompanies.Select(pc => new ProductionCompanyRequest(
                pc.Name,
                pc.ImdbId,
                pc.TmdbId
              )),
            ],
          },
          cancellationToken
        );

        if (syncResult.IsFailure)
        {
          Log_SyncFailed(_logger, item.PublicId, syncResult.Error!.ToString());
          failed++;
          continue;
        }

        succeeded++;
      }
      catch (ScreenDraftsException ex)
      {
        Log_Exception(_logger, item.PublicId, ex.ToString());
        failed++;
      }
    }

    Log_ResyncComplete(_logger, succeeded, failed);
  }

  private sealed record IncompleteMediaRecord(
    int? TmdbId,
    int? IgdbId,
    string? ImdbId,
    int MediaTypeValue,
    int? TvSeriesTmdbId,
    int? SeasonNumber,
    int? EpisodeNumber,
    string PublicId
  );

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "MediaPeopleResync: no incomplete media found — skipping."
  )]
  private static partial void Log_NothingToResync(ILogger<MediaPeopleResyncSeeder> logger);

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "MediaPeopleResync: starting resync for {Count} media records."
  )]
  private static partial void Log_StartingResync(
    ILogger<MediaPeopleResyncSeeder> logger,
    int count
  );

  [LoggerMessage(
    Level = LogLevel.Warning,
    Message = "MediaPeopleResync: failed to fetch online data for {PublicId} — skipping."
  )]
  private static partial void Log_FetchFailed(
    ILogger<MediaPeopleResyncSeeder> logger,
    string publicId
  );

  [LoggerMessage(
    Level = LogLevel.Error,
    Message = "MediaPeopleResync: sync failed for {PublicId}. Error: {Error}"
  )]
  private static partial void Log_SyncFailed(
    ILogger<MediaPeopleResyncSeeder> logger,
    string publicId,
    string error
  );

  [LoggerMessage(
    Level = LogLevel.Error,
    Message = "MediaPeopleResync: exception processing {PublicId}. Message: {Message}"
  )]
  private static partial void Log_Exception(
    ILogger<MediaPeopleResyncSeeder> logger,
    string publicId,
    string message
  );

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "MediaPeopleResync: complete. Succeeded: {Succeeded}, Failed: {Failed}."
  )]
  private static partial void Log_ResyncComplete(
    ILogger<MediaPeopleResyncSeeder> logger,
    int succeeded,
    int failed
  );
}
