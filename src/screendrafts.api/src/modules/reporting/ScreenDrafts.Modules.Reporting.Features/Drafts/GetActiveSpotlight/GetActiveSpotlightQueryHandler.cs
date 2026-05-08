namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetActiveSpotlight;

internal sealed class GetActiveSpotlightQueryHandler(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService,
  IMovieTitleReader movieTitleReader
) : IQueryHandler<GetActiveSpotlightQuery, GetActiveSpotlightResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IMovieTitleReader _movieTitleReader = movieTitleReader;

  private static readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

  public async Task<Result<GetActiveSpotlightResponse>> Handle(
    GetActiveSpotlightQuery request,
    CancellationToken cancellationToken
  )
  {
    var cached = await _cacheService.GetAsync<GetActiveSpotlightResponse>(
      ReportingCacheKeys.SpotlightCacheKey,
      cancellationToken
    );

    if (cached is not null)
    {
      return Result.Success(cached);
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string spotlightSql = """
      SELECT
        ds.draft_public_id                AS DraftPublicId,
        ds.spotlight_description          AS SpotlightDescription,
        ds.spotify_url                    AS SpotifyUrl,
        s.title                           AS Title,
        s.episode_number                  AS EpisodeNumber,
        s.draft_type                      AS DraftType,
        s.total_parts                     AS TotalParts,
        s.total_picks                     AS TotalPicks
      FROM reporting.draft_spotlights ds
      JOIN reporting.draft_summaries s ON ds.draft_public_id = s.draft_public_id
      WHERE ds.is_active = TRUE
        AND s.is_complete =  TRUE
      LIMIT 1
      """;

    var header = await connection.QuerySingleOrDefaultAsync<SpotlightHeaderRow>(
      new CommandDefinition(commandText: spotlightSql, cancellationToken: cancellationToken)
    );

    if (header is null)
    {
      return Result.Failure<GetActiveSpotlightResponse>(DraftReportingErrors.NoActiveSpotlight);
    }

    const string picksSql = """
      SELECT
        mcp.board_position        AS Position,
        mcp.movie_public_id       AS MediaPublicId
      FROM reporting.movie_canonical_picks mcp
      JOIN reporting.draft_summaries s ON mcp.draft_part_public_id = s.draft_part_public_id
      WHERE s.draft_public_id = @DraftPublicId
      ORDER BY mcp.board_position ASC
      LIMIT 5
      """;

    var pickRows = (
      await connection.QueryAsync<PickRow>(
        new CommandDefinition(
          commandText: picksSql,
          parameters: new { header.DraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var publicIds = pickRows.Select(r => r.MediaPublicId).ToList();

    var titleMap = await _movieTitleReader.GetTitlesByPublicIdsAsync(publicIds, cancellationToken);

    var picks = pickRows
      .Select(r => new SpotlightPickResponse
      {
        Position = r.Position,
        MediaPublicId = r.MediaPublicId,
        MediaTitle =
          titleMap.GetValueOrDefault(r.MediaPublicId, r.MediaPublicId) ?? "Unknown Title",
      })
      .ToList()
      .AsReadOnly();

    var response = new GetActiveSpotlightResponse
    {
      DraftPublicId = header.DraftPublicId,
      Title = header.Title,
      EpisodeNumber = header.EpisodeNumber,
      DraftType = header.DraftType,
      TotalParts = header.TotalParts,
      SpotlightDescription = header.SpotlightDescription,
      SpotifyUrl = header.SpotifyUrl,
      TopPicks = picks,
      TotalPicks = header.TotalPicks,
    };

    await _cacheService.SetAsync(
      ReportingCacheKeys.SpotlightCacheKey,
      response,
      _cacheExpiration,
      cancellationToken
    );

    return Result.Success(response);
  }

  private sealed record SpotlightHeaderRow(
    string DraftPublicId,
    string SpotlightDescription,
    string? SpotifyUrl,
    string Title,
    int? EpisodeNumber,
    string DraftType,
    int TotalParts,
    int TotalPicks
  );

  private sealed record PickRow(int Position, string MediaPublicId);
}
