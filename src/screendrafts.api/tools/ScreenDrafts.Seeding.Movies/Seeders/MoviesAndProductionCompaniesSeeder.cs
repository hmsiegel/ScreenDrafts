namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class MoviesAndProductionCompaniesSeeder(
  MoviesDbContext dbContext,
  ILogger<MoviesAndProductionCompaniesSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 11;

  public string Name => "moviesproductioncompanies";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedMoviesAndProductionCompaniesAsync(cancellationToken);
  }

  private async Task SeedMoviesAndProductionCompaniesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "MoviesProductionCompanies";
    const string DBTableName = $"{Schemas.Movies}.{Tables.MovieProductionCompanies}";

    var csvMovieProductionCompanies = await ReadRawLinesAsync(
      new SeedFile(FileNames.MovieProductionCompaniesSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvMovieProductionCompanies,
      DBTableName,
      [ColumnNames.MovieId, ColumnNames.ProductionCompanyId],
      values =>
      [
        Guid.Parse(values[0].Trim()),
        Guid.Parse(values[1].Trim()),
      ],
      _sqlInsertHelper,
      cancellationToken);
  }
}
