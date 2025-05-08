namespace ScreenDrafts.Seeding.Drafts.Seeders.Drafts;

internal sealed class DraftStatsSeeder(
  ILogger<DraftStatsSeeder> logger,
  ICsvFileService csvFileService,
  DraftsDbContext dbContext,
  SqlInsertHelper sqlInsertHelper)
  : DraftBaseSeeder(dbContext, logger, csvFileService), ICustomSeeder
{
  public int Order => 10;

  public string Name => "drafterdraftstats";

  private readonly SqlInsertHelper _sqlInsertHelper = sqlInsertHelper;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedDraftStatsAsync(cancellationToken);
  }

  private async Task SeedDraftStatsAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DrafterDraftStats";
    const string DbTableName = $"{Schemas.Drafts}.{Tables.DrafterDraftStats}";

    var csvDraftStats = await ReadRawLinesAsync(
      new SeedFile(FileNames.DraftStatsSeeder, SeedFileType.RawLines),
      TableName,
      cancellationToken);

    await InsertFromLinesAsync(
      csvDraftStats,
      DbTableName,
      [
        ColumnNames.DraftId,
        ColumnNames.DrafterId,
        ColumnNames.DrafterTeamId,
        ColumnNames.StartingVetoes,
        ColumnNames.RolloverVetoes,
        ColumnNames.RolloverVetoOverrides,
        ColumnNames.TriviaVetoes,
        ColumnNames.TriviaVetoOverrides,
        ColumnNames.CommissionerOverrides,
        ColumnNames.VetoesUsed,
        ColumnNames.VetoOverridesUsed
      ],
      values =>
      [
        Guid.Parse(values[0].Trim()),
        Guid.Parse(values[1].Trim()),
        Guid.Parse(values[2].Trim()),
        int.Parse(values[3].Trim(), CultureInfo.InvariantCulture),
        int.Parse(values[4].Trim(), CultureInfo.InvariantCulture),
        int.Parse(values[5].Trim(), CultureInfo.InvariantCulture),
        int.Parse(values[6].Trim(), CultureInfo.InvariantCulture),
        int.Parse(values[7].Trim(), CultureInfo.InvariantCulture),
        int.Parse(values[8].Trim(), CultureInfo.InvariantCulture),
        int.Parse(values[9].Trim(), CultureInfo.InvariantCulture),
        int.Parse(values[10].Trim(), CultureInfo.InvariantCulture)
      ],
      _sqlInsertHelper,
      cancellationToken);

    LogInsertComplete(TableName, csvDraftStats.Length - 1);
  }
}
