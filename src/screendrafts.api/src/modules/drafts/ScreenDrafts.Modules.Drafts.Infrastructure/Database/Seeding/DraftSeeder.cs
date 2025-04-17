using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Seeding;

internal sealed class DraftSeeder(DraftsDbContext dbContext, ILogger<DraftSeeder> logger) : ICustomSeeder
{
  private readonly DraftsDbContext _dbContext = dbContext;
  private readonly ILogger<DraftSeeder> _logger = logger;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    var filePath = Path.Combine(dataPath, FileNames.DraftSeeder);
    var draftDraftersFilePath = Path.Combine(dataPath, FileNames.DraftDraftersSeeder);
    var draftsAndDrafterTeamsFilePath = Path.Combine(dataPath, FileNames.DraftsTeamsSeeder);

    await SeedDraftsAsync(filePath, cancellationToken);
    await SeedDraftAndDraftersAsync(draftDraftersFilePath, cancellationToken);
    await SeedDraftsAndDrafterTeamsAsync(draftsAndDrafterTeamsFilePath, cancellationToken);
  }

  private async Task SeedDraftsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "Drafts";

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var json = await File.ReadAllTextAsync(filePath, cancellationToken);
    var drafts = JsonSerializer.Deserialize<List<DraftsModel>>(json, SerializerOptions.Instance);

    if (drafts is null)
    {
      return;
    }

    var knownDrafts = drafts.Where(d => d.Id.HasValue).ToList();
    var draftIds = knownDrafts.Select(draft => DraftId.Create(draft.Id!.Value)).ToList();

    var existingDraftIds = await _dbContext.Drafts
      .Where(draft => draftIds.Contains(draft.Id))
      .Select(draft => draft.Id)
      .ToHashSetAsync(cancellationToken);

    var newDrafts = drafts.Where(draft =>
      !draft.Id.HasValue || !existingDraftIds.Contains(DraftId.Create(draft.Id!.Value))).ToList();

    if (newDrafts.Count == 0)
    {
      DatabaseSeedingLoggingMessages.AlreadySeeded(_logger, TableName);
      return;
    }

    foreach (var draft in newDrafts)
    {
      var id = draft.Id.HasValue ? DraftId.Create(draft.Id.Value) : DraftId.CreateUnique();

      var currentDraft = Draft.Create(
        title: Title.Create(draft.Title),
        draftType: DraftType.FromValue(draft.DraftType),
        totalPicks: draft.TotalPicks,
        totalDrafters: draft.TotalDrafters,
        totalDrafterTeams: draft.TotalDrafterTeams,
        totalHosts: draft.TotalHosts,
        episodeType: EpisodeType.FromValue(draft.EpisodeType),
        draftStatus: DraftStatus.FromValue(draft.DraftStatus),
        id: id).Value;

      _dbContext.Drafts.Add(currentDraft);

      var existingDates = currentDraft.ReleaseDates.Select(rd => rd.ReleaseDate).ToHashSet();
      var newDates = draft.ReleaseDates
        .Where(date => !existingDates.Contains(date))
        .Select(date => DraftReleaseDate.Create(currentDraft.Id, date))
        .ToList();

      foreach (var date in newDates)
      {
        currentDraft.AddReleaseDate(date);
      }

      if (draft.IsPatreonOnly)
      {
        currentDraft.SetPatreonOnly(draft.IsPatreonOnly);
      }

      if (!draft.EpisodeNumber.IsNullOrWhiteSpace())
      {
        currentDraft.SetEpisodeNumber(draft.EpisodeNumber!);
      }

      if (draft.IsNonCanonical)
      {
        currentDraft.SetNonCanonical(draft.IsNonCanonical);
      }

      AddGameBoard(currentDraft);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, currentDraft.Title.Value);
    }

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, drafts.Count, filePath, TableName);

    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  private void AddGameBoard(Draft draft)
  {
    var gameBoard = GameBoard.Create(draft);

    _dbContext.GameBoards.Add(gameBoard.Value);
  }

  private async Task SeedDraftAndDraftersAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "DraftDrafters";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvDraftDrafters = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvDraftDrafters.Skip(1))
    {
      var values = line.Split(',');

      if (values.Length < 2)
      {
        continue;
      }

      var draftId = Guid.Parse(values[1].Trim());
      var drafterId = Guid.Parse(values[0].Trim());

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO drafts.drafts_drafters (draft_id, drafter_id)
        VALUES ({0}, {1})
        ON CONFLICT DO NOTHING
        """,
        draftId,
        drafterId);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, $"DraftId: {draftId}, DrafterId: {drafterId}");

    }
    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvDraftDrafters.Length, filePath, TableName);
  }
  private async Task SeedDraftsAndDrafterTeamsAsync(string filePath, CancellationToken cancellationToken)
  {
    const string TableName = "DraftsTeams";

    DatabaseSeedingLoggingMessages.StartingSeeding(_logger, TableName);

    if (!File.Exists(filePath))
    {
      DatabaseSeedingLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var csvDraftsTeams = await File.ReadAllLinesAsync(filePath, cancellationToken);

    foreach (var line in csvDraftsTeams.Skip(1))
    {
      var values = line.Split(',');
      if (values.Length < 2)
      {
        continue;
      }

      var draftId = Guid.Parse(values[0].Trim());
      var drafterTeamId = Guid.Parse(values[1].Trim());

      await _dbContext.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO drafts.drafts_drafter_teams (draft_id, drafter_team_id)
        VALUES ({0}, {1})
        ON CONFLICT DO NOTHING
        """,
        draftId,
        drafterTeamId);

      DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, $"DraftId: {draftId}, DrafterTeamId: {drafterTeamId}");

    }

    DatabaseSeedingLoggingMessages.BulkInsertMessage(_logger, csvDraftsTeams.Length, filePath, TableName);
  }
}
