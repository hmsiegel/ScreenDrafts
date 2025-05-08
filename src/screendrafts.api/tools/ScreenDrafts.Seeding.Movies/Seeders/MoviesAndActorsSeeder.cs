namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class MoviesAndActorsSeeder(
  MoviesDbContext dbContext,
  ILogger<MoviesAndActorsSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 6;

  public string Name => "moviesandactors";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesAndActorsAsync(cancellationToken);
  }

  private async Task SeedMoviesAndActorsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "MovieActors";
    const string DbTableName = $"{Schemas.Movies}.{Tables.MovieGenres}";

    var csvMovieActors = await ReadRawLinesAsync(
      new SeedFile(FileNames.MovieActorsSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvMovieActors,
      DbTableName,
      [ColumnNames.MovieId, ColumnNames.ActorId, ColumnNames.Id],
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
