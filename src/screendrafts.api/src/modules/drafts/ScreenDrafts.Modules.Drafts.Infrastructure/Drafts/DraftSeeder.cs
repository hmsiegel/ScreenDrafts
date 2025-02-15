using System.Text.Json;

using Quartz.Util;

using ScreenDrafts.Modules.Drafts.Infrastructure.Serialization;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftSeeder(
  ILogger<DrafterSeeder> logger,
  DraftsDbContext dbContext,
  IDraftsRepository draftsRepository) : ICustomSeeder
{
  private readonly ILogger<DrafterSeeder> _logger = logger;
  private readonly DraftsDbContext _dbContext = dbContext;
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
      ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    var filePath = Path.Combine(dataPath, "drafts.json");

    DatabaseLoggingMessages.StartingSeeding(_logger);

    await SeedDraftsAsync(filePath!, cancellationToken);

    DatabaseLoggingMessages.SeedingComplete(_logger);
  }

  private async Task SeedDraftsAsync(string filePath, CancellationToken cancellationToken)
  {
    if (!File.Exists(filePath))
    {
      DatabaseLoggingMessages.FileNotFound(_logger, filePath);
      return;
    }

    var json = await File.ReadAllTextAsync(filePath, cancellationToken);
    var drafts = JsonSerializer.Deserialize<List<DraftsModel>>(json, SerializerOptions.Instance);

    if (drafts is null)
    {
      return;
    }

    foreach (var draft in drafts)
    {
      var currentDraft = await _draftsRepository.GetByIdAsync(DraftId.Create(draft.Id), cancellationToken);
      if (currentDraft is null)
      {
        currentDraft = Draft.Create(
          id: DraftId.Create(draft.Id),
          title: Title.Create(draft.Title),
          draftType: DraftType.FromValue(draft.DraftType),
          totalPicks: draft.TotalPicks,
          totalDrafters: draft.TotalDrafters,
          totalHosts: draft.TotalHosts,
          episodeType: EpisodeType.FromValue(draft.EpisodeType),
          draftStatus: DraftStatus.FromValue(draft.DraftStatus)).Value;

        _draftsRepository.Add(currentDraft);
      }

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
    }
    DatabaseLoggingMessages.BulkInsertMessage(_logger, drafts.Count, filePath);

    await _dbContext.SaveChangesAsync(cancellationToken);

  }
}
