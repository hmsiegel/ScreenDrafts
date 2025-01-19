namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

internal sealed class ListDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IRequestHandler<ListDraftsQuery, IEnumerable<DraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<IEnumerable<DraftResponse>> Handle(ListDraftsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
        SELECT
          id AS {nameof(DraftResponse.Id)},
          title AS {nameof(DraftResponse.Title)},
          draft_type AS {nameof(DraftResponse.DraftType)},
          number_of_drafters AS {nameof(DraftResponse.NumberOfDrafters)},
          number_of_commissioners AS {nameof(DraftResponse.NumberOfCommissioners)},
          number_of_movies AS {nameof(DraftResponse.NumberOfMovies)}
        FROM drafts.drafts
        """;

    IEnumerable<DraftResponse> drafts = await connection.QueryAsync<DraftResponse>(sql);

    return drafts;
  }
}
