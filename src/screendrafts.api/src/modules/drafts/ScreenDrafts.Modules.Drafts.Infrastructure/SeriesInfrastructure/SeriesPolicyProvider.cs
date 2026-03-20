using Microsoft.Extensions.Caching.Memory;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.SeriesInfrastructure;

internal sealed class SeriesPolicyProvider(DraftsDbContext dbContext, IMemoryCache memoryCache) : ISeriesPolicyProvider
{
  private readonly DraftsDbContext _dbContext = dbContext;
  private readonly IMemoryCache _memoryCache = memoryCache;

  private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

  public async Task<Series?> GetSeriesAsyc(SeriesId seriesId, CancellationToken cancellationToken)
  {
    var cacheKey = $"drafts:series:{seriesId.Value}";

    if (_memoryCache.TryGetValue(cacheKey, out Series? cachedSeries))
    {
      return cachedSeries;
    }

    var series = await _dbContext.Series.FirstOrDefaultAsync(s => s.Id == seriesId, cancellationToken);

    if (series != null)
    {
      _memoryCache.Set(cacheKey, series, _cacheDuration);
    }

    return series;
  }
}
