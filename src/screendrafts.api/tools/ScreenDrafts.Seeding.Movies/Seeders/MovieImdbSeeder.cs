using ValidationError = ScreenDrafts.Common.Abstractions.Errors.ValidationError;

namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed partial class MovieImdbSeeder(
  MoviesDbContext dbContext,
  ILogger<MovieImdbSeeder> logger,
  ICsvFileService csvFileService,
  ISender sender,
  IPublicIdGenerator publicIdGenerator)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 1;

  public string Name => "moviestmdb";

  private readonly ISender _sender = sender;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMediaAsync(cancellationToken);
  }

  private async Task SeedMediaAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Movies";

    var csvRows = ReadCsv<MovieCsvModel>(
      new SeedFile(FileNames.MovieSeeder, SeedFileType.Csv),
      TableName);

    if (csvRows.Count == 0)
    {
      return;
    }

    var tmdbRows = csvRows
      .Where(r => r.TmdbId.HasValue)
      .Select(r => new { r.TmdbId, r.MediaType })
      .ToList();

    var tmdbIdList = tmdbRows.Select(r => r.TmdbId!.Value).ToList();

    var igdbIds = csvRows
      .Where(r => r.IgdbId.HasValue)
      .Select(r => r.IgdbId!.Value)
      .ToList();

    var imdbIds = csvRows
      .Where(r => !r.TmdbId.HasValue && !r.IgdbId.HasValue && !string.IsNullOrWhiteSpace(r.ImdbId))
      .Select(r => r.ImdbId!)
      .ToList();

    var existingTmdbPairs = tmdbIdList.Count > 0
      ? [.. (await _dbContext.Media
          .Where(m => m.TmdbId.HasValue && tmdbIdList.Contains(m.TmdbId!.Value))
          .Select(m => new { m.TmdbId, MediaTypeValue = m.MediaType.Value })
          .ToListAsync(cancellationToken))
          .Select(m => (m.TmdbId!.Value, m.MediaTypeValue))]
      : new HashSet<(int, int)>();

    var existingIgdbIds = igdbIds.Count > 0
      ? await _dbContext.Media
          .Where(m => m.IgdbId.HasValue && igdbIds.Contains(m.IgdbId!.Value))
          .Select(m => m.IgdbId!.Value)
          .ToHashSetAsync(cancellationToken)
      : [];

    var existingImdbIds = imdbIds.Count > 0
      ? await _dbContext.Media
          .Where(m => m.ImdbId != null && imdbIds.Contains(m.ImdbId))
          .Select(m => m.ImdbId!)
          .ToHashSetAsync(cancellationToken)
      : [];

    var newRows = csvRows.Where(r =>
      (r.TmdbId.HasValue && !existingTmdbPairs.Contains((r.TmdbId.Value, r.MediaType))) ||
      (r.IgdbId.HasValue && !existingIgdbIds.Contains(r.IgdbId.Value)) ||
      (!r.TmdbId.HasValue && !r.IgdbId.HasValue && !string.IsNullOrWhiteSpace(r.ImdbId) && !existingImdbIds.Contains(r.ImdbId)))
      .ToList();

    if (newRows.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

#pragma warning disable S3267 // Loop performs side effects and cannot be simplified
    foreach (var row in newRows)
    {
      try
      {
        var mediaType = MediaType.FromValue(row.MediaType);

        var fetchCommand = new GetOnlineMediaCommand
        {
          MediaType = mediaType,
          TmdbId = row.TmdbId,
          IgdbId = row.IgdbId,
          ImdbId = row.ImdbId,
          TvSeriesTmdbId = row.TvSeriesTmdbId,
          SeasonNumber = row.SeasonNumber,
          EpisodeNumber = row.EpisodeNumber
        };
        var fetchResponse = await _sender.Send(fetchCommand, cancellationToken);

        if (fetchResponse is null || fetchResponse.IsFailure)
        {
          var id = row.TmdbId?.ToString(CultureInfo.InvariantCulture)
            ?? row.IgdbId?.ToString(CultureInfo.InvariantCulture)
            ?? row.ImdbId
            ?? "unknown";

          Log_FailedToFetchMedia(id);
          continue;
        }


        var r = fetchResponse.Value;
        var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Media);

        var addCommand = new AddMediaCommand
        {
          PublicId = publicId,
          ImdbId = r.ImdbId,
          TmdbId = r.TmdbId,
          IgdbId = r.IgdbId,
          Title = r.Title,
          Year = r.Year,
          Plot = r.Plot,
          Image = r.Image,
          ReleaseDate = r.ReleaseDate,
          YouTubeTrailerUrl = r.YouTubeTrailerUrl,
          MediaType = r.MediaType,
          TvSeriesTmdbId = r.TvSeriesTmdbId,
          SeasonNumber = r.SeasonNumber,
          EpisodeNumber = r.EpisodeNumber,
          Genres = [.. r.Genres.Select(g => new GenreRequest(g.TmdbId, g.Name))],
          Directors = [.. r.Directors.Select(d => new PersonRequest(d.Name, d.ImdbId, d.TmdbId))],
          Actors = [.. r.Actors.Select(a => new PersonRequest(a.Name, a.ImdbId, a.TmdbId))],
          Writers = [.. r.Writers.Select(w => new PersonRequest(w.Name, w.ImdbId, w.TmdbId))],
          Producers = [.. r.Producers.Select(p => new PersonRequest(p.Name, p.ImdbId, p.TmdbId))],
          ProductionCompanies = [.. r.ProductionCompanies.Select(pc => new ProductionCompanyRequest(pc.Name, pc.ImdbId, pc.TmdbId))]
        };


        var addResult = await _sender.Send(addCommand, cancellationToken);

        if (addResult.IsFailure)
        {
          var errorMessages = addResult.Errors
            .SelectMany(e => e is ValidationError ve
              ? ve.Errors.Select(innerError => $"{innerError.Code}: {innerError.Description}")
              : [$"{e.Code}: {e.Description}"]);

          var id = row.TmdbId?.ToString(CultureInfo.InvariantCulture)
            ?? row.IgdbId?.ToString(CultureInfo.InvariantCulture)
            ?? row.ImdbId
            ?? "unknown";

          Log_FailedToAddMedia(id, fetchResponse.Value.Title,
            string.Join(", ", errorMessages));
          continue;
        }
      }
      catch (ScreenDraftsException ex)
      {
        var id = row.TmdbId?.ToString(CultureInfo.InvariantCulture)
          ?? row.IgdbId?.ToString(CultureInfo.InvariantCulture)
          ?? row.ImdbId
          ?? "unknown";
        Log_ErrorFetchingMedia(ex.Message, id);
      }
    }
#pragma warning restore S3267
  }

  [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to fetch media details for ID: {id}. Skipping.")]
  private partial void Log_FailedToFetchMedia(string id);

  [LoggerMessage(Level = LogLevel.Error, Message = "An error occurred while fetching media details for ID: {id}. Error message: {message}. Skipping.")]
  private partial void Log_ErrorFetchingMedia(string message, string id);

  [LoggerMessage(Level = LogLevel.Error, Message = "Failed to add media with ID: {id} - Title: {title}. Error: {errors}")]
  private partial void Log_FailedToAddMedia(string id, string title, string errors);
}
