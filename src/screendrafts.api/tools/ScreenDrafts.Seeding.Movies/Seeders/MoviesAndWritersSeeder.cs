namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class MoviesAndWritersSeeder(
  MoviesDbContext dbContext,
  ILogger<MoviesAndWritersSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 7;

  public string Name => "moviesandwriters";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesAndWritersAsync(cancellationToken);
  }

  private async Task SeedMoviesAndWritersAsync(CancellationToken cancellationToken)
  {
    const string TableName = "MovieWriters";
    const string DbTableName = $"{Schemas.Movies}.{Tables.MovieWriters}";

    var csvMovieWriters = await ReadRawLinesAsync(
      new SeedFile(FileNames.MovieActorsSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvMovieWriters,
      DbTableName,
      [
        ColumnNames.MovieId,
        ColumnNames.WriterId,
        ColumnNames.Id
      ],
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
