namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed class GetCategoryQueryHandler (IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetCategoryQuery, CategoryResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery GetCategoryRequest, CancellationToken cancellationToken)
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
        WHERE
          c.publicId = @PublicId;
      """;

    var result = await connection.QuerySingleOrDefaultAsync<CategoryResponse>(new CommandDefinition(
      sql,
      new { GetCategoryRequest.PublicId },
      cancellationToken: cancellationToken));

    return result is not null
      ? Result.Success(result)
      : Result.Failure<CategoryResponse>(CategoryErrors.NotFound(GetCategoryRequest.PublicId));
  }
}



