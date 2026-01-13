using ScreenDrafts.Common.Features.Abstractions.CsvFiles;
using ScreenDrafts.Common.Features.Abstractions.Seeding;

namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftDraftersSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftDraftersSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public int Order => 7;

  public string Name => "draftsdrafters";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftAndDraftersAsync(cancellationToken);
  }

  private async Task SeedDraftAndDraftersAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftDrafters";
    const string DBTableName = $"{Schemas.Drafts}.{Tables.DraftsDrafters}";

    var csvDraftDrafters = await ReadRawLinesAsync(
      new SeedFile(FileNames.DraftDraftersSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvDraftDrafters,
      DBTableName,
      [ColumnNames.DrafterId, ColumnNames.DraftId],
      values =>
      [
        Guid.Parse(values[0].Trim()),
        Guid.Parse(values[1].Trim())
      ],
      _sqlInsertHelper,
      cancellationToken);

    LogInsertComplete(TableName, csvDraftDrafters.Length - 1);
  }
}
