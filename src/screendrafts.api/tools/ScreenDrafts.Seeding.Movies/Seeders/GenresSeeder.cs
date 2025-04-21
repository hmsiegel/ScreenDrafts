namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class GenresSeeder(
  MoviesDbContext dbContext,
  ILogger<GenresSeeder> logger,
  ICsvFileService csvFileService) 
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 4;

  public string Name => "genres";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedGenresExportAsync(cancellationToken);
  }

  private async Task SeedGenresExportAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Genres";


    var csvGenres = ReadCsv<GenreExportCsvModel>(
      new SeedFile(FileNames.GenreSeeder, SeedFileType.Csv),
      TableName);

    if (csvGenres.Count == 0)
    {
      return;
    }

    await InsertIfNotExistsAsync(
      csvGenres,
      genre => Guid.Parse(genre.Id),
      genre => genre.Id,
      genre => Genre.Create(
        name: genre.Name,
        id: Guid.Parse(genre.Id)),
      _dbContext.Genres,
      TableName,
      cancellationToken);
  }
}
