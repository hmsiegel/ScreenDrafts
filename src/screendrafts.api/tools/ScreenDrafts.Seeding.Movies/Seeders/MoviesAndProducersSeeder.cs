namespace ScreenDrafts.Seeding.Movies.Seeders;
internal sealed class MoviesAndProducersSeeder(
  MoviesDbContext dbContext,
  ILogger<MoviesAndProducersSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 9;

  public string Name => "moviesandproducers";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesAndProducersAsync(cancellationToken);
  }

  private async Task SeedMoviesAndProducersAsync(CancellationToken cancellationToken)
  {
    const string TableName = "MovieProducers";
    const string DBTableName = $"{Schemas.Movies}.{Tables.MovieProducers}";

    var csvMovieProducers = await ReadRawLinesAsync(
      new SeedFile(FileNames.MovieProducersSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvMovieProducers,
      DBTableName,
      [ColumnNames.MovieId, ColumnNames.ProducerId, ColumnNames.Id],
      values =>
      [
        Guid.Parse(values[0].Trim()),
        Guid.Parse(values[1].Trim()),
        Guid.CreateVersion7()
      ],
      _sqlInsertHelper,
      cancellationToken);
  }
}
