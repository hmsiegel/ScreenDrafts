namespace ScreenDrafts.Seeding.Drafts.Seeders.People;

internal sealed class TeamsDrafterSeeder(
  ILogger<TeamsDrafterSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext,
  SqlInsertHelper sqlInsertHelper)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 5;

  public string Name => "drafterteamsdrafter";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDrafterAndDrafterTeamsAsync(cancellationToken);
  }

  private async Task SeedDrafterAndDrafterTeamsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DrafterTeamsDrafter";
    const string DbTableName = $"{Schemas.Drafts}.{Tables.DrafterTeamDrafter}";

    var csvDrafterTeamsDrafter = await ReadRawLinesAsync(
      new SeedFile(FileNames.DrafterTeamsDraftersSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvDrafterTeamsDrafter,
      DbTableName,
      [ColumnNames.DrafterId, ColumnNames.DrafterTeamId],
      values =>
      [
        Guid.Parse(values[1].Trim()),
        Guid.Parse(values[0].Trim())
      ],
      _sqlInsertHelper,
      cancellationToken);

    LogInsertComplete(TableName, csvDrafterTeamsDrafter.Length - 1);
  }
}
