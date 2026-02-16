namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftDrafterTeamsSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftDrafterTeamsSeeder> logger,
  ICsvFileService csvFileService,
  SqlInsertHelper sqlInsertHelper)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 8;

  public string Name => "draftsdrafterteams";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftsAndDrafterTeamsAsync(cancellationToken);
  }

  private async Task SeedDraftsAndDrafterTeamsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftsTeams";
    const string DbTableName = $"{Schemas.Drafts}.{Tables.DrafterTeamsDrafts}";

    var file = new SeedFile(FileNames.DraftsTeamsSeeder, SeedFileType.RawLines);
    var csvDraftsTeams = await ReadRawLinesAsync(
      file,
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvDraftsTeams,
      DbTableName,
      [ColumnNames.DraftId, ColumnNames.DrafterTeamId],
      values =>
      [
        Guid.Parse(values[0].Trim()),
        Guid.Parse(values[1].Trim())
      ],
      _sqlInsertHelper,
      cancellationToken);

    LogInsertComplete(TableName, csvDraftsTeams.Length - 1);
  }
}
