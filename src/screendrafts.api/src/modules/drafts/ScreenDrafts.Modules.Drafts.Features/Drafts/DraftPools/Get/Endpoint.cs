namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDraftPoolRequest, DraftPoolResponse>
{
  public override void Configure()
  {
    Get(DraftRoutes.Pool);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftPools)
      .WithName(DraftsOpenApi.Names.DraftPools_GetPool)
      .Produces<DraftPoolResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPoolRead);
  }

  public override async Task HandleAsync(GetDraftPoolRequest req, CancellationToken ct)
  {
    var query = new GetDraftPoolQuery 
    {
      PublicId = req.PublicId
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

internal sealed record DraftPoolResponse
{
  public string PublicId { get; init; } = default!;
  public string DraftId { get; init; } = default!;
  public bool IsLocked { get; init; }
  public IReadOnlyList<int> TmdbIds { get; init; } = [];
}

internal sealed record GetDraftPoolRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}
internal sealed record GetDraftPoolQuery : IQuery<DraftPoolResponse>
{
  public required string PublicId { get; init; }
}

internal sealed class GetDraftPoolQueryHandler(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService)
  : IQueryHandler<GetDraftPoolQuery, DraftPoolResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;

  private static readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(60);

  public async Task<Result<DraftPoolResponse>> Handle(GetDraftPoolQuery request, CancellationToken cancellationToken)
  {
    var cacheKey = DraftsCacheKeys.DraftPool(request.PublicId);

    var cached = await _cacheService.GetAsync<DraftPoolResponse>(cacheKey, cancellationToken);

    if (cached is not null)
    {
      return Result.Success(cached);
    }

    const string poolSql =
      $"""
      SELECT
        p.public_id AS {nameof(DraftPoolResponse.PublicId)},
        d.public_id AS {nameof(DraftPoolResponse.DraftId)},
        p.is_locked AS {nameof(DraftPoolResponse.IsLocked)}
      FROM drafts.draft_pools p
      JOIN drafts.drafts d ON p.draft_id = d.id
      WHERE d.public_id = @PublicId
      LIMIT 1;
      """;

    const string itemsSql =
      $"""
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
      new CommandDefinition(
        poolSql,
        new { request.PublicId },
        cancellationToken: cancellationToken));

    if (pool is null)
    {
      return Result.Failure<DraftPoolResponse>(DraftPoolErrors.NotFound(request.PublicId));
    }

    var tmdbIds = (await connection.QueryAsync<int>(
      new CommandDefinition(
        itemsSql,
        new { request.PublicId },
        cancellationToken: cancellationToken)))
      .ToList()
      .AsReadOnly();

    var response = pool with { TmdbIds = tmdbIds };

    await _cacheService.SetAsync(
      cacheKey,
      response,
      _cacheExpiration,
      cancellationToken);

    return Result.Success(response);
  }
}
