using ScreenDrafts.Modules.Reporting.PublicApi;

namespace ScreenDrafts.Modules.Reporting.Infrastructure.PublicApi;

internal sealed class ReportingApi(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService
) : IReportingApi
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;

  private const string CacheKey = "reporting:drafter-honorific:";
  private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

  public async Task<DrafterHonorificResponse?> GetDrafterHonorificAsync(
    Guid drafterInternalId,
    CancellationToken cancellationToken = default
  )
  {
    var cached = await _cacheService.GetAsync<DrafterHonorificResponse>(
      $"{CacheKey}{drafterInternalId}",
      cancellationToken
    );

    if (cached is not null)
    {
      return cached;
    }

    await using var connections = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        dh.honorific            AS {nameof(HonorificRow.HonorificValue)},
        dh.appearance_count     AS {nameof(HonorificRow.AppearanceCount)}
      FROM
        reporting.drafter_honorifics dh
      WHERE
        dh.drafter_id_value = @DrafterInternalId
      """;

    var row = await connections.QuerySingleOrDefaultAsync<HonorificRow>(
      new CommandDefinition(
        sql,
        new { DrafterInternalId = drafterInternalId },
        cancellationToken: cancellationToken
      )
    );

    if (row is null || row.HonorificValue == 0)
    {
      return null;
    }

    var honorific = DrafterHonorific.FromValue(row.HonorificValue);

    var result = new DrafterHonorificResponse
    {
      HonorificValue = row.HonorificValue,
      HonorificName = honorific.ToString(),
      AppearanceCount = row.AppearanceCount,
    };

    await _cacheService.SetAsync(
      $"{CacheKey}{drafterInternalId}",
      result,
      _cacheDuration,
      cancellationToken
    );
    return result;
  }

  public async Task<IReadOnlyList<Guid>> GetDrafterIdsByHonorificAsync(
    int honorificValue,
    CancellationToken cancellationToken = default
  )
  {
    var cacheKey = $"{CacheKey}{honorificValue}";

    var cached = await _cacheService.GetAsync<List<Guid>>(cacheKey, cancellationToken);

    if (cached is not null)
    {
      return cached;
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT drafter_id_value
      FROM reporting.drafter_honorifics
      WHERE honorific = @HonorificValue
      """;

    var ids = (
      await connection.QueryAsync<Guid>(
        new CommandDefinition(
          sql,
          new { HonorificValue = honorificValue },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    await _cacheService.SetAsync(cacheKey, ids, TimeSpan.FromHours(1), cancellationToken);

    return ids;
  }

  private sealed record HonorificRow(int HonorificValue, int AppearanceCount);
}
