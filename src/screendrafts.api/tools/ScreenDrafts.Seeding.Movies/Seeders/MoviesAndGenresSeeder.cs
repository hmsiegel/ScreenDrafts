namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class MoviesAndGenresSeeder(
  MoviesDbContext dbContext,
  ILogger<MoviesAndGenresSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 10;

  public string Name => "moviesandgenres";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesAndGenresAsync(cancellationToken);
  }

  private async Task SeedMoviesAndGenresAsync(CancellationToken cancellationToken)
  {
    const string TableName = "MoviesGenres";
    const string DbTableName = $"{Schemas.Movies}.{Tables.MovieGenres}";

    var csvMovieGenres = await ReadRawLinesAsync(
      new SeedFile(FileNames.MovieGenreSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvMovieGenres,
      DbTableName,
      [ColumnNames.MovieId, ColumnNames.GenreId],
      values =>
      [
        Guid.Parse(values[0].Trim()),
        Guid.Parse(values[1].Trim())
      ],
      _sqlInsertHelper,
      cancellationToken);
  }
}
