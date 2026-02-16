using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Categories;

internal sealed class CategorySeeder(
  DraftsDbContext dbContext,
  ILogger<CategorySeeder> logger,
  ICsvFileService csvFileService,
  IPublicIdGenerator publicIdGenerator)
  : DraftBaseSeeder(
    dbContext,
    logger,
    csvFileService), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  public int Order => 2;
  public string Name => "categories";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedCategoriesAsync(cancellationToken);

  private async Task SeedCategoriesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Categories";

    var rows = ReadCsv<CategoryCsvModel>(
      new SeedFile(FileNames.CategoriesSeeder, SeedFileType.Csv),
      TableName);

    if (rows is null || rows.Count == 0)
    {
      return;
    }

    var knownCategories = rows.Where(r => r.Id.HasValue).ToList();

    var ids = knownCategories.Select(x => CategoryId.Create(x.Id!.Value)).ToList();

    await _dbContext.Categories
      .Where(c => ids.Contains(c.Id) &&
      (c.PublicId == null || c.PublicId == string.Empty))
      .ExecuteUpdateAsync(setters => setters
      .SetProperty(c => c.PublicId,
      c => _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Category)),
        cancellationToken);

    var existing = await _dbContext.Categories
      .Where(c => ids.Contains(c.Id))
      .Select(c => c.Id)
      .ToHashSetAsync(cancellationToken);

    var toAdd = rows.Where(r =>
    !r.Id.HasValue || !existing.Contains(CategoryId.Create(r.Id.Value))).ToList();

    if (toAdd.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var row in toAdd)
    {
      var id = row.Id.HasValue ? CategoryId.Create(row.Id!.Value) : CategoryId.CreateUnique();
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Category);

      var category = Category.Create(
        id: id,
        publicId: publicId,
        name: row.Name,
        description: row.Description
      ).Value;

      _dbContext.Categories.Add(category);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, $"Category '{category.Name}'");
    }
    await SaveAndLogAsync(TableName, toAdd.Count);
  }
}
