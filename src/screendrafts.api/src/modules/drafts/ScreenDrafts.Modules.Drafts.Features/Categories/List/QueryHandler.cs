namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<Query, CategoryCollectionResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<CategoryCollectionResponse>> Handle(
    Query request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        SELECT
          c.publicId AS {nameof(CategoryResponse.PublicId)},
          c.name AS {nameof(CategoryResponse.Name)},
          c.description AS {nameof(CategoryResponse.Description)}
        FROM
          drafts.categories c
        ORDER BY
          c.name ASC
      """;

    var categories = (await connection.QueryAsync<CategoryResponse>(
      new CommandDefinition(sql, cancellationToken: cancellationToken)
    )).ToList();

    var response = new CategoryCollectionResponse(categories);

    return Result.Success(response);
  }
}

