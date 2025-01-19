namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

internal sealed class GetDraftQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IRequestHandler<GetDraftQuery, DraftResponse?>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<DraftResponse?> Handle(GetDraftQuery request, CancellationToken cancellationToken)
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
        WHERE id = @DraftId
        """;

    DraftResponse? draft = await connection.QuerySingleOrDefaultAsync<DraftResponse>(sql, new { request.DraftId });

    return draft;
  }
}
