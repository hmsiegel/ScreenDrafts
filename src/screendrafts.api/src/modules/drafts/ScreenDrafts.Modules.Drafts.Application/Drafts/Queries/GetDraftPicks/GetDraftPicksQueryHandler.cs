namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;

internal sealed class GetDraftPicksByDraftQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetDraftPicksByDraftQuery, IEnumerable<DraftPickResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IEnumerable<DraftPickResponse>>> Handle(GetDraftPicksByDraftQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
        SELECT
          position AS {nameof(DraftPickResponse.Position)},
          movie_id AS {nameof(DraftPickResponse.MovieId)},
          drafter_id AS {nameof(DraftPickResponse.DrafterId)},
          draft_id AS {nameof(DraftPickResponse.DraftId)}
          FROM drafts.picks
          WHERE draft_id = @DraftId
      """;

    List<DraftPickResponse> draftPicks = [.. (await connection.QueryAsync<DraftPickResponse>(sql, request))];

    return draftPicks;
  }
}
