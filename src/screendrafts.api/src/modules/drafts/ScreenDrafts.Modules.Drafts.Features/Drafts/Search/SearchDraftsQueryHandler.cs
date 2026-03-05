namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Search;

internal sealed class SearchDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<SearchDraftsQuery, PagedResult<SearchDraftsResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PagedResult<SearchDraftsResponse>>> Handle(SearchDraftsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseSql =
      $"""
      SELECT
        d.public_id AS {nameof(SearchDraftsResponse.PublicId)},
        d.title AS {nameof(SearchDraftsResponse.Title)},
        d.draft_type AS {nameof(SearchDraftsResponse.DraftType)},
        d.draft_status AS {nameof(SearchDraftsResponse.DraftStatus)},
        c.public_id AS {nameof(SearchDraftsResponse.CampaignPublicId)},
        c.name AS {nameof(SearchDraftsResponse.CampaignName)},
        s.public_id AS {nameof(SearchDraftsResponse.SeriesPublicId)},
        s.name AS {nameof(SearchDraftsResponse.SeriesName)}
      FROM drafts.drafts d
      JOIN drafts.series s ON d.series_id = s.id
      LEFT JOIN drafts.campaigns c ON d.campaign_id = c.id
      WHERE 1 = 1
      """;

    var sqlBuilder = new StringBuilder(baseSql);
    var parameters = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.Name))
    {
      sqlBuilder.Append(" AND d.title ILIKE '%' || @name || '%'");
      parameters.Add("name", request.Name);
    }

    if (!string.IsNullOrWhiteSpace(request.CampaignPublicId))
    {
      sqlBuilder.Append(" AND c.public_id = @campaignPublicId");
      parameters.Add("campaignPublicId", request.CampaignPublicId);
    }

    if (!string.IsNullOrWhiteSpace(request.CategoryPublicId))
    {
      sqlBuilder.Append(
        """
         AND EXISTS (
           SELECT 1
           FROM drafts.draft_categories dc
           JOIN drafts.categories cat ON dc.category_id = cat.id
           WHERE dc.draft_id = d.id AND cat.public_id = @categoryPublicId
         )
        """);
      parameters.Add("categoryPublicId", request.CategoryPublicId);
    }

    if (request.DraftType.HasValue)
    {
      sqlBuilder.Append(" AND d.draft_type = @draftType");
      parameters.Add("draftType", request.DraftType.Value);
    }

    sqlBuilder.Append(" ORDER BY d.title ASC");

    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        $"SELECT COUNT(*) FROM ({sqlBuilder}) sub",
        parameters,
        cancellationToken: cancellationToken));

    var pageSize = Math.Min(request.PageSize, 100);
    var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

    parameters.Add("pageSize", pageSize);
    parameters.Add("skip", skip);
    sqlBuilder.Append(" LIMIT @pageSize OFFSET @skip");

    var items = (await connection.QueryAsync<SearchDraftsResponse>(
      new CommandDefinition(
        sqlBuilder.ToString(),
        parameters,
        cancellationToken: cancellationToken)))
      .ToList();

    return Result.Success(new PagedResult<SearchDraftsResponse>
    {
      Items = items,
      TotalCount = totalCount,
      Page = request.Page,
      PageSize = pageSize
    });
  }
}
