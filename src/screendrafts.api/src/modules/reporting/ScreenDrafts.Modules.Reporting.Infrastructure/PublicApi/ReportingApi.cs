using Microsoft.Extensions.Caching.Distributed;
using ScreenDrafts.Modules.Reporting.PublicApi;

namespace ScreenDrafts.Modules.Reporting.Infrastructure.PublicApi;

internal sealed class ReportingApi(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService
) : IReportingApi
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;

  private const string DrafterHonorificCacheKey = "reporting:drafter-honorific:";
  private const string MediaHonorificCacheKey = "reporting:media-honorific:";
  private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

  public async Task<DrafterHonorificResponse?> GetDrafterHonorificAsync(
    Guid drafterInternalId,
    CancellationToken cancellationToken = default
  )
  {
    var cached = await _cacheService.GetAsync<DrafterHonorificResponse>(
      $"{DrafterHonorificCacheKey}{drafterInternalId}",
      cancellationToken
    );

    if (cached is not null)
    {
      return cached;
    }

    await using var connections = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        dh.honorific            AS {nameof(DrafterHonorificRow.HonorificValue)},
        dh.appearance_count     AS {nameof(DrafterHonorificRow.AppearanceCount)}
      FROM
        reporting.drafter_honorifics dh
      WHERE
        dh.drafter_id_value = @DrafterInternalId
      """;

    var row = await connections.QuerySingleOrDefaultAsync<DrafterHonorificRow>(
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
      $"{DrafterHonorificCacheKey}{drafterInternalId}",
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
    var cacheKey = $"{DrafterHonorificCacheKey}{honorificValue}";

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

  public async Task<MediaHonorificRecord?> GetMediaHonorificAsync(
    string mediaPublicId,
    CancellationToken ct = default
  )
  {
    var cacheKey = $"{MediaHonorificCacheKey}{mediaPublicId}";

    var cached = await _cacheService.GetAsync<MediaHonorificRecord>(cacheKey, ct);

    if (cached is not null)
    {
      return cached;
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(ct);

    const string sql = """
      SELECT
          appearance_honorific    AS AppearanceHonorificValue,
          position_honorific      AS PositionHonorificValue,
          appearance_count        AS AppearanceCount
      FROM reporting.movie_honorifics
      WHERE movie_public_id = @MediaPublicId
      """;

    var row = await connection.QuerySingleOrDefaultAsync<DraftHonorificRow>(
      sql,
      new { MediaPublicId = mediaPublicId }
    );

    if (row is null)
      return null;

    // Map appearance_honorific int to its name
    var appearanceName = row.AppearanceHonorificValue switch
    {
      1 => "Marquee of Fame",
      2 => "Hat Trick",
      3 => "Grand Slam",
      4 => "High Five",
      _ => "None",
    };

    var record = new MediaHonorificRecord
    {
      AppearanceHonorificValue = row.AppearanceHonorificValue,
      AppearanceHonorificName = appearanceName,
      PositionHonorificValue = row.PositionHonorificValue,
      AppearanceCount = row.AppearanceCount,
    };

    await _cacheService.SetAsync(cacheKey, record, TimeSpan.FromHours(1), ct);

    return record;
  }

  private sealed record DrafterHonorificRow(int HonorificValue, int AppearanceCount);

  private sealed record DraftHonorificRow(
    int AppearanceHonorificValue,
    int PositionHonorificValue,
    int AppearanceCount
  );
}
