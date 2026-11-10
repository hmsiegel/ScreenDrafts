namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMedia;

internal sealed class ListMediaQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListMediaQuery, ListMediaResponse>
{
  public async Task<Result<ListMediaResponse>> Handle(
    ListMediaQuery request,
    CancellationToken cancellationToken
  )
  {
    // Role only matters alongside a person. A malformed key or unknown role fails loudly
    // rather than silently widening the filter.
    PersonKey? personKey = null;
    IReadOnlyList<MediaCreditRoles.CreditSource> creditSources = [];
    if (!string.IsNullOrWhiteSpace(request.PersonKey))
    {
      if (!PersonKey.TryParse(request.PersonKey, out var parsedKey))
      {
        return Result.Failure<ListMediaResponse>(MediaErrors.InvalidPersonKey);
      }

      if (!MediaCreditRoles.TryResolve(request.Role, out creditSources))
      {
        return Result.Failure<ListMediaResponse>(MediaErrors.InvalidCreditRole);
      }

      personKey = parsedKey;
    }

    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var page = request.Page < 1 ? 1 : request.Page;
    var pageSize = request.PageSize is < 1 or > 100 ? 24 : request.PageSize;
    var offset = (page - 1) * pageSize;

    var (whereClause, parameters) = BuildWhere(request, creditSources, personKey);
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
        MediaTypeName = MediaType.FromValue(r.MediaTypeValue).Name,
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

  private static (string Clause, DynamicParameters Params) BuildWhere(
    ListMediaQuery request,
    IReadOnlyList<MediaCreditRoles.CreditSource> creditSources,
    PersonKey? personKey
  )
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

    if (personKey is { } key && creditSources.Count > 0)
    {
      // One EXISTS per credit table, OR-ed together. Table and column names come from
      // MediaCreditRoles' fixed list; the person identity is a bound parameter. A person
      // is matched by TMDb ID when they have one, otherwise by IMDb ID -- about four in
      // five people predate the TMDb import and have tmdb_id = 0.
      var exists = creditSources.Select(
        (s, i) =>
        {
          var match = key.TmdbId.HasValue
            ? $"p{i}.tmdb_id = @PersonTmdbId"
            : $"p{i}.imdb_id = @PersonImdbId";

          return $"EXISTS (SELECT 1 FROM {s.Table} c{i} JOIN movies.people p{i} ON p{i}.id = c{i}.{s.PersonColumn} WHERE c{i}.media_id = m.id AND {match})";
        }
      );

      conditions.Add($"({string.Join(" OR ", exists)})");

      if (key.TmdbId.HasValue)
      {
        p.Add("PersonTmdbId", key.TmdbId.Value);
      }
      else
      {
        p.Add("PersonImdbId", key.ImdbId);
      }
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
