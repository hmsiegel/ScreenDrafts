using ScreenDrafts.Modules.Drafts.Domain.Hosts;

namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class HostsDraftsSeeder(
  ILogger<HostsDraftsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext,
  SqlInsertHelper sqlInsertHelper) : DraftBaseSeeder(
    dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 6;

  public string Name => "draftshosts";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedHostsDraftsAsync(cancellationToken);
  }

  private async Task SeedHostsDraftsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftsHosts";
    const string DbTableName = $"{Schemas.Drafts}.{Tables.DraftsHosts}";

    var csvHostsDrafts = await ReadRawLinesAsync(
      new SeedFile(FileNames.HostsDraftsSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvHostsDrafts,
      DbTableName,
      [ColumnNames.DraftPartId, ColumnNames.HostId, ColumnNames.Role],
      values =>
      [
        Guid.Parse(values[0].Trim()),
        Guid.Parse(values[1].Trim()),
        int.Parse(values[2].Trim(), CultureInfo.InvariantCulture)
      ],
      _sqlInsertHelper,
      cancellationToken);

    LogInsertComplete(TableName, csvHostsDrafts.Length - 1);
  }
}
