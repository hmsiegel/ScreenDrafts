namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;

internal sealed class GetLatestDraftQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetLatestDraftsQuery, IReadOnlyList<LatestDraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  public async Task<Result<IReadOnlyList<LatestDraftResponse>>> Handle(GetLatestDraftsQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
    const string sql =
      $@"
            SELECT
              id AS {nameof(Draft.Id)},
              title AS {nameof(Draft.Title)},
              episode_number AS {nameof(Draft.EpisodeNumber)},
              release_date AS {nameof(Draft.ReleaseDate)}
            FROM drafts.drafts
            ORDER BY release_date
            LIMIT 5
            ";
    IReadOnlyList<LatestDraftResponse> drafts = (await connection.QueryAsync<LatestDraftResponse>(sql)).ToList();
    return Result.Success(drafts);
  }
}
