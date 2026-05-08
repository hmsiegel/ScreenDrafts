namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetSiteStats;

internal sealed class GetSiteStatsQueryHandler(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService
) : IQueryHandler<GetSiteStatsQuery, GetSiteStatsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;

  private static readonly TimeSpan _staticCacheDuration = TimeSpan.FromHours(24);
  private static readonly TimeSpan _episodeCacheDuration = TimeSpan.FromHours(1);

  public async Task<Result<GetSiteStatsResponse>> Handle(
    GetSiteStatsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var staticStats = await _cacheService.GetAsync<StaticStatsRow>(
      ReportingCacheKeys.StaticCacheKey,
      cancellationToken
    );

    if (staticStats is null)
    {
      const string staticSql = """
        SELECT
          ss.vetoes_count AS VetoesCount,
          Cast((SELECT COUNT(DISTINCT movie_public_id)
            FROM  reporting.movie_canonical_picks) as int4) AS FilmsDrafted,
          Cast((SELECT COUNT(DISTINCT drafter_id_value)
            FROM reporting.drafter_canonical_appearances) as int4) AS GuestGMs,
          Cast((SELECT COUNT(*)
            FROM reporting.drafter_honorifics
            WHERE honorific = 4) as int4) AS Legends
        FROM reporting.site_stats ss
        LIMIT 1
        """;

      staticStats = await connection.QuerySingleOrDefaultAsync<StaticStatsRow>(
        new CommandDefinition(staticSql, cancellationToken: cancellationToken)
      );

      if (staticStats is not null)
      {
        await _cacheService.SetAsync(
          key: ReportingCacheKeys.StaticCacheKey,
          value: staticStats,
          expiration: _staticCacheDuration,
          cancellationToken: cancellationToken
        );
      }
    }

    var episodeCacheKey = request.IsPatreonMember
      ? ReportingCacheKeys.PatreonEpisodeCacheKey
      : ReportingCacheKeys.PublicEpisodeCacheKey;

    var episodeCount = await _cacheService.GetAsync<int?>(episodeCacheKey, cancellationToken);

    if (episodeCount is null)
    {
      const string publicEpisodeSql = """
        SELECT COUNT(DISTINCT draft_part_public_id)
        FROM reporting.draft_part_releases
        WHERE release_channel = 'MainFeed'
        """;

      const string patreonEpisodeSql = """
        SELECT COUNT(*)
        FROM reporting.draft_summaries
        """;

      var sql = request.IsPatreonMember ? patreonEpisodeSql : publicEpisodeSql;

      episodeCount = await connection.ExecuteScalarAsync<int>(
        new CommandDefinition(sql, cancellationToken: cancellationToken)
      );

      await _cacheService.SetAsync(
        key: episodeCacheKey,
        value: episodeCount,
        expiration: _episodeCacheDuration,
        cancellationToken: cancellationToken
      );
    }

    return Result.Success(
      new GetSiteStatsResponse
      {
        EpisodesProduced = episodeCount.Value,
        FilmsDrafted = staticStats?.FilmsDrafted ?? 0,
        GuestGMs = staticStats?.GuestGMs ?? 0,
        VetoesDeployed = staticStats?.VetoesCount ?? 0,
        Legends = staticStats?.Legends ?? 0,
      }
    );
  }

  private sealed record StaticStatsRow(
    int VetoesCount,
    int FilmsDrafted,
    int GuestGMs,
    int Legends
  );
}
