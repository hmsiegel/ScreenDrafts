namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetLatestDrafts;

internal sealed class GetLatestDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
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
              release_date AS {nameof(DraftReleaseDate.ReleaseDate)}
            FROM drafts.drafts d
            JOIN draft_release_dates drd ON d.id = drd.draft_id
            WHERE d.draft_status = 'Completed'
            ORDER BY drd.release_date DESC
            LIMIT 5
            ";

    List<LatestDraftResponse> drafts = [.. (await connection.QueryAsync<LatestDraftResponse>(sql))];

    return drafts;
  }
}
