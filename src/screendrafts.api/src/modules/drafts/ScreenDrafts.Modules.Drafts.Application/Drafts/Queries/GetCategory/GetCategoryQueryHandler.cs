namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetCategory;

internal sealed class GetCategoryQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetCategoryQuery, CategoryResponse?>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<CategoryResponse?>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        SELECT
          c.id AS {nameof(CategoryResponse.Id)},
          c.name AS {nameof(CategoryResponse.Name)},
          c.description AS {nameof(CategoryResponse.Description)}
        FROM drafts.categories c
        WHERE c.id = @Id;
        """;

    var category = await connection.QuerySingleOrDefaultAsync<CategoryResponse?>(sql, new { request.Id });

    if (category is null)
    {
      return Result.Failure<CategoryResponse?>(CategoryErrors.NotFound(request.Id));
    }

    return category;
  }
}
