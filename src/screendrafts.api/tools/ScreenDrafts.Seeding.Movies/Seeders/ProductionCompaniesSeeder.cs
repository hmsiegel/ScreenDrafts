namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed class ProductionCompaniesSeeder(
  MoviesDbContext dbContext,
  ILogger<ProductionCompaniesSeeder> logger,
  ICsvFileService csvFileService) 
  : MovieBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 5;

  public string Name => "productioncompanies";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedProductionCompaniesExportAsync(cancellationToken);
  }

  private async Task SeedProductionCompaniesExportAsync(CancellationToken cancellationToken)
  {
    const string TableName = "ProductionCompanies";

    var csvProductionCompanies = ReadCsv<ProductionCompanyExportCsvModel>(
      new SeedFile(FileNames.ProductionCompanySeeder, SeedFileType.Csv),
      TableName);

    if (csvProductionCompanies.Count == 0)
    {
      return;
    }

    await InsertIfNotExistsAsync(
      csvProductionCompanies,
      productionCompany => Guid.Parse(productionCompany.Id),
      productionCompany => productionCompany.Id,
      productionCompany => ProductionCompany.Create(
        productionCompany.Name,
        productionCompany.ImdbId,
        Guid.Parse(productionCompany.Id)),
      _dbContext.ProductionCompanies,
      TableName,
      cancellationToken);
  }
}
