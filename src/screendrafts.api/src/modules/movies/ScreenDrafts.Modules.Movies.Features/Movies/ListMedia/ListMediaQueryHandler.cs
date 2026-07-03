namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed class ListMediaQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListMediaQuery, ListMediaResponse>
{
  private static readonly Dictionary<int, string> _mediaTypeNames = new()
  {
    { 0, "Movie" },
    { 1, "TvShow" },
    { 2, "TvEpisode" },
    { 3, "VideoGame" },
    { 4, "MusicVideo" },
  };

  public async Task<Result<ListMediaResponse>> Handle(
    ListMediaQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var page = request.Page < 1 ? 1 : request.Page;
    var pageSize = request.PageSize is < 1 or > 100 ? 24 : request.PageSize;
    var offset = (page - 1) * pageSize;

    var (whereClause, parameters) = BuildWhere(request);
    var orderClause = BuildOrder(request.Sort);

    var countSql = $"""
      SELECT COUNT(*)
      FROM movies.media m
      {whereClause}
      """;

    var dataSql = $"""
      SELECT
          m.public_id   AS {nameof(MediaListItemResponse.PublicId)},
          m.title       AS {nameof(MediaListItemResponse.Title)},
          m.year        AS {nameof(MediaListItemResponse.Year)},
          m.media_type  AS {nameof(MediaListItemResponse.MediaTypeValue)},
          m.image       AS {nameof(MediaListItemResponse.Image)},
          m.imdb_id     AS {nameof(MediaListItemResponse.ImdbId)},
          m.tmdb_id     AS {nameof(MediaListItemResponse.TmdbId)}
      FROM movies.media m
      {whereClause}
      {orderClause}
      LIMIT @PageSize OFFSET @Offset
      """;

    parameters.Add("PageSize", pageSize);
    parameters.Add("Offset", offset);

    // S2077: countSql/dataSql interpolate whereClause and orderClause, both built from fixed literal fragments/whitelisted switches; all values are bound via Dapper parameters.
#pragma warning disable S2077
    var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

    var rows = await connection.QueryAsync<MediaListItemRow>(dataSql, parameters);
#pragma warning restore S2077

    var items = rows.Select(r => new MediaListItemResponse
      {
        PublicId = r.PublicId,
        Title = r.Title,
        Year = r.Year,
        MediaTypeValue = r.MediaTypeValue,
        MediaTypeName = _mediaTypeNames.GetValueOrDefault(r.MediaTypeValue, "Movie"),
        Image = r.Image,
        ImdbId = r.ImdbId,
        TmdbId = r.TmdbId,
      })
      .ToList();

    return Result.Success(
      new ListMediaResponse
      {
        Result = new PagedResult<MediaListItemResponse>
        {
          Items = items,
          TotalCount = totalCount,
          Page = page,
          PageSize = pageSize,
        },
      }
    );
  }

  private static (string Clause, DynamicParameters Params) BuildWhere(ListMediaQuery request)
  {
    var conditions = new List<string>();
    var p = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.Search))
    {
      conditions.Add("m.title ILIKE @Search");
      p.Add("Search", $"%{request.Search.Trim()}%");
    }

    if (request.MediaType.HasValue)
    {
      conditions.Add("m.media_type = @MediaType");
      p.Add("MediaType", request.MediaType.Value);
    }

    if (!string.IsNullOrWhiteSpace(request.Year))
    {
      conditions.Add("m.year LIKE @Year");
      p.Add("Year", $"{request.Year.Trim()}%");
    }

    var clause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

    return (clause, p);
  }

  private static string BuildOrder(string? sort) =>
    sort switch
    {
      "title_asc" => "ORDER BY m.title ASC",
      "title_desc" => "ORDER BY m.title DESC",
      "year_asc" => "ORDER BY m.year ASC NULLS LAST",
      "year_desc" => "ORDER BY m.year DESC NULLS LAST",
      _ => "ORDER BY m.title ASC",
    };

  private sealed record MediaListItemRow(
    string PublicId,
    string Title,
    string? Year,
    int MediaTypeValue,
    string? Image,
    string? ImdbId,
    int? TmdbId
  );
}
