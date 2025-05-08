namespace ScreenDrafts.Seeding.Movies.Seeders;
internal sealed class MoviesAndDirectorsSeeder(
  MoviesDbContext dbContext,
  ILogger<MoviesAndDirectorsSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 8;

  public string Name => "moviesanddirectors";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesAndDirectorsAsync(cancellationToken);
  }

  private async Task SeedMoviesAndDirectorsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "MovieDirectors";
    const string DbTableName = $"{Schemas.Movies}.{Tables.MovieGenres}";

    var csvMovieDirectors = await ReadRawLinesAsync(
      new SeedFile(FileNames.MovieDirectorsSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvMovieDirectors,
      DbTableName,
      [ColumnNames.MovieId, ColumnNames.DirectorId, ColumnNames.Id],
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
