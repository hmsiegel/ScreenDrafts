using ScreenDrafts.Modules.Integrations.Domain.Services.Tmdb;

namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed partial class TmdbIdBackfillSeeder(
  MoviesDbContext dbContext,
  ITmdbService tmdbService,
  ILogger<TmdbIdBackfillSeeder> logger)
  : ICustomSeeder
{
  private readonly MoviesDbContext _dbContext = dbContext;
  private readonly ITmdbService _tmdbService = tmdbService;
  private readonly ILogger<TmdbIdBackfillSeeder> _logger = logger;

  public int Order => 0;
  public string Name => "tmdbidbackfill";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
    => await BackfilAsync(cancellationToken);

  private async Task BackfilAsync(CancellationToken cancellationToken)
  {
    // Load all rows with an imdb_id regardless of whether they have a tmdb_id or not
    // we also want to correct MediaType on rows that were defaulted to Movie but are actually TV shows

    var rows = await _dbContext.Media
      .Where(m => m.ImdbId != null)
      .ToListAsync(cancellationToken);

    if (rows.Count == 0)
    {
      Log_NothingToBackfill(_logger);
      return;
    }

    Log_StartingBackfill(_logger, rows.Count);
    var updated = 0;
    var skipped = 0;
    var unknown = 0;

    foreach (var media in rows)
    {
      try
      {
        var findResult = await _tmdbService.FindByImdbIdAsync(
          media.ImdbId!,
          cancellationToken);

        if (findResult is null)
        {
          Log_NotFoundOnTmdb(_logger, media.ImdbId!, media.Title);
          unknown++;
          continue;
        }

        if (findResult.IsUnknown)
        {
          Log_NotFoundOnTmdb(_logger, media.ImdbId!, media.Title);
          unknown++;
          continue;
        }

        var needsSave = false;
        if (findResult.HasMovie)
        {
          var r = findResult.MovieResult!;

          if (media.TmdbId != r.Id)
          {
            media.SetTmdbId(r.Id);
            needsSave = true;
          }

          if (media.MediaType != MediaType.Movie)
          {
            media.SetMediaType(MediaType.Movie);
            needsSave = true;
          }
        }
        else if (findResult.HasTvShow)
        {
          var r = findResult.TvResult!;
          if (media.TmdbId != r.Id)
          {
            media.SetTmdbId(r.Id);
            needsSave = true;
          }

          if (media.MediaType != MediaType.TvShow)
          {
            media.SetMediaType(MediaType.TvShow);
            needsSave = true;
          }
        }
        else if (findResult.HasTvEpisode)
        {
          var r = findResult.TvEpisodeResult!;
          if (media.TmdbId != r.Id)
          {
            media.SetTmdbId(r.Id);
            needsSave = true;
          }

          if (media.MediaType != MediaType.TvEpisode)
          {
            media.SetMediaType(MediaType.TvEpisode);
            needsSave = true;
          }

          if (media.TvSeriesTmdbId != r.ShowId)
          {
            media.SetEpisodeDetails(r.ShowId, r.SeasonNumber, r.EpisodeNumber);
            needsSave = true;
          }
        }
        else
        {
          // This should be impossible since we check for unknown above, but just in case
          Log_NotFoundOnTmdb(_logger, media.ImdbId!, media.Title);
          unknown++;
          continue;
        }

        if (needsSave)
        {
          updated++;
        }
        else
        {
          skipped++;
        }

        // Batch save every 50 updates to avoid overwhelming the change tracker and database
        if (updated > 0 && updated % 50 == 0)
        {
          await _dbContext.SaveChangesAsync(cancellationToken);
          Log_Progress(_logger, updated, rows.Count);
          await Task.Delay(100, cancellationToken); // Small delay to avoid overwhelming the server
        }
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
        Log_BackfillError(_logger, ex, media.ImdbId!, media.Title, ex.Message);
      }
    }

    if (_dbContext.ChangeTracker.HasChanges())
    {
      await _dbContext.SaveChangesAsync(cancellationToken);
    }

    Log_BackfillComplete(_logger, updated, skipped, unknown, rows.Count);
  }


  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Information,
    Message = "No media entries with an IMDb ID were found to backfill with TMDb IDs.")]
  private static partial void Log_NothingToBackfill(ILogger<TmdbIdBackfillSeeder> logger);

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Starting backfill of TMDb IDs for {Count} media entries with IMDb IDs.")]
  private static partial void Log_StartingBackfill(ILogger<TmdbIdBackfillSeeder> logger, int count);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Error,
    Message = "IMDb ID '{imdbId}' ({title}) not found on TMDb - likely a game or bad ID. Review manually.")]
  private static partial void Log_NotFoundOnTmdb(ILogger<TmdbIdBackfillSeeder> logger, string imdbId, string title);

  [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Information,
    Message = "Progress: {updated}/{total} rows updated.")]
  private static partial void Log_Progress(ILogger<TmdbIdBackfillSeeder> logger, int updated, int total);

  [LoggerMessage(
    EventId = 4,
    Level = LogLevel.Error,
    Message = "Error backfilling IMDb ID '{imdbId}' ({title}): {errorMessage}")]
  private static partial void Log_BackfillError(ILogger<TmdbIdBackfillSeeder> logger, Exception exception, string imdbId, string title, string errorMessage);

  [LoggerMessage(
    EventId = 5,
    Level = LogLevel.Information,
    Message = "Backfill complete. Updated: {updated}, Already correct: {skipped}, Not found on TMDb: {unknown}, Total: {total}")]
  private static partial void Log_BackfillComplete(ILogger<TmdbIdBackfillSeeder> logger, int updated, int skipped, int unknown, int total);
}
