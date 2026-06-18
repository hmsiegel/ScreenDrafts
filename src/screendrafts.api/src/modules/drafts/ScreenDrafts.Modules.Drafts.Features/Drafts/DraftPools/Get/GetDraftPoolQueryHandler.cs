namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Get;

internal sealed class GetDraftPoolQueryHandler(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService
) : IQueryHandler<GetDraftPoolQuery, DraftPoolResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;

  private static readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(60);

  public async Task<Result<DraftPoolResponse>> Handle(
    GetDraftPoolQuery request,
    CancellationToken cancellationToken
  )
  {
    var cacheKey = DraftsCacheKeys.DraftPool(request.PublicId);

    var cached = await _cacheService.GetAsync<DraftPoolResponse>(cacheKey, cancellationToken);

    if (cached is not null)
    {
      return Result.Success(cached);
    }

    const string poolSql = $"""
      SELECT
        p.public_id AS {nameof(DraftPoolResponse.PublicId)},
        d.public_id AS {nameof(DraftPoolResponse.DraftId)},
        p.is_locked AS {nameof(DraftPoolResponse.IsLocked)}
      FROM drafts.draft_pools p
      JOIN drafts.drafts d ON p.draft_id = d.id
      WHERE d.public_id = @PublicId
      LIMIT 1;
      """;

    const string itemsSql = $"""
      SELECT
        i.tmdb_id AS TmdbId
      FROM drafts.draft_pool_items i
      JOIN drafts.draft_pools p ON i.draft_pool_id = p.id
      JOIN drafts.drafts d ON p.draft_id = d.id
      WHERE d.public_id = @PublicId
      ORDER BY i.tmdb_id ASC;
      """;

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var pool = await connection.QuerySingleOrDefaultAsync<DraftPoolResponse>(
      new CommandDefinition(poolSql, new { request.PublicId }, cancellationToken: cancellationToken)
    );

    if (pool is null)
    {
      return Result.Failure<DraftPoolResponse>(DraftPoolErrors.NotFound(request.PublicId));
    }

    var tmdbIds = (
      await connection.QueryAsync<int>(
        new CommandDefinition(
          itemsSql,
          new { request.PublicId },
          cancellationToken: cancellationToken
        )
      )
    )
      .ToList()
      .AsReadOnly();

    var response = pool with { TmdbIds = tmdbIds };

    await _cacheService.SetAsync(cacheKey, response, _cacheExpiration, cancellationToken);

    return Result.Success(response);
  }
}
