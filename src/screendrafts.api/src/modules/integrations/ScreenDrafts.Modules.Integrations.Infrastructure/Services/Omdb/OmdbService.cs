namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Omdb;

internal sealed partial class OmdbService(
  IOptions<OmdbSettings> omdbSettings,
  ILogger<OmdbService> logger
) : IOmdbService
{
  private readonly OmdbSettings _omdbSettings = omdbSettings.Value;
  private readonly ILogger<OmdbService> _logger = logger;

  private AsyncOmdbClient OmdbClient => new(_omdbSettings.Key);

  public async Task<Item> GetItemByTitleAsync(string title, bool fullPlot) =>
    await OmdbClient.GetItemByTitleAsync(title, fullPlot);

  public async Task<Item> GetItemByIdAsync(string id, bool fullPlot) =>
    await OmdbClient.GetItemByIdAsync(id, fullPlot);

  public async Task<Item> GetSeriesByTitleAsync(string id, bool fullPlot) =>
    await OmdbClient.GetItemByTitleAsync(id, OmdbType.Series, fullPlot);

  public async Task<SearchList?> SearchAsync(string query, int page)
  {
    try
    {
      return await OmdbClient.GetSearchListAsync(query, page);
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
    {
      LogOmdbSearchFailed(_logger, query, page, ex.ToString());
      return null;
    }
  }

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Warning,
    Message = "OMDb search failed for query {Query}, page {Page} — treating as no results. Exception: {Exception}"
  )]
  private static partial void LogOmdbSearchFailed(
    ILogger<OmdbService> logger,
    string query,
    int page,
    string exception
  );
}
