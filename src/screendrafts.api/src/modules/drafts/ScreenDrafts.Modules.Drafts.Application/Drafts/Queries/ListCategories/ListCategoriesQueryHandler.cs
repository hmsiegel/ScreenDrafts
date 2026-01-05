
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListCategories;

internal sealed class ListCategoriesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListCategoriesQuery, PagedResult<CategoryResponse?>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PagedResult<CategoryResponse?>>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseSql =
      $"""
        SELECT
          c.id AS {nameof(CategoryResponse.Id)},
          c.name AS {nameof(CategoryResponse.Name)},
          c.description AS {nameof(CategoryResponse.Description)}
        FROM drafts.categories c
        WHERE 1 = 1
        """;

    var sql = new StringBuilder(baseSql);
    var parameters = new DynamicParameters();

    if (!string.IsNullOrWhiteSpace(request.Q))
    {
      sql.Append(" AND (c.name ILIKE @q OR c.description ILIKE @q)");
      parameters.Add("q", $"%{request.Q.Trim()}%");
    }

    sql.Append(" ORDER BY c.name ASC");

    var totalCount = await connection.ExecuteScalarAsync<int>(
      $"""
      SELECT COUNT(*)
      FROM ({baseSql})
      """, parameters);

    var skip = (request.Page <= 1 ? 0 : request.Page - 1) * request.PageSize;
    parameters.Add("pageSize", request.PageSize > 100 ? 100 : request.PageSize); // Limit to 100 for performance
    parameters.Add("skip", skip);

    sql.Append(" LIMIT @pageSize OFFSET @skip");

    var categories = (await connection.QueryAsync<CategoryResponse>(sql.ToString(), parameters)).ToList();

    if (categories.Count == 0)
    {
      return null!;
    }

    return new PagedResult<CategoryResponse?>(
      Items: categories,
      Total: totalCount,
      Page: request.Page,
      PageSize: request.PageSize);
  }
}
