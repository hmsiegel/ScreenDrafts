namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed class ListLatestDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListLatestDraftsQuery, ListLatestDraftsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  private const int CompletedStatus = 3;
  private const int MainFeedChannel = 0;

  public async Task<Result<ListLatestDraftsResponse>> Handle(ListLatestDraftsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var baseSql = new StringBuilder(
      $"""
      SELECT
        dp.public_id AS {nameof(LatestDraftResponse.DraftPartPublicId)},
        d.public_id AS {nameof(LatestDraftResponse.DraftPublicId)},
        d.title AS {nameof(LatestDraftResponse.Title)},
        dp.part_index AS {nameof(LatestDraftResponse.PartNumber)},
        (SELECT COUNT(*) FROM drafts.draft_parts dp2 WHERE dp2.draft_id = d.id) AS {nameof(LatestDraftResponse.TotalParts)},
        MIN(r.release_date) AS {nameof(LatestDraftResponse.ReleaseDate)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON dp.draft_id = d.id
      LEFT JOIN drafts.draft_releases r ON r.part_id = dp.id
      WHERE dp.status = {CompletedStatus}
      """
      );

    if (!request.IncludePatreonOnly)
    {
      baseSql.Append(CultureInfo.InvariantCulture,$" AND r.release_channel = {MainFeedChannel} ");
    }

    baseSql.Append(
      $"""

      GROUP BY
        dp.id, dp.public_id, dp.part_index,
        d.id, d.public_id, d.title
      ORDER BY MIN(r.release_date) DESC NULLS LAST
      LIMIT 10
      """
      );

    IReadOnlyCollection<LatestDraftResponse> results = [.. await connection.QueryAsync<LatestDraftResponse>(
      sql: baseSql.ToString(),
      param: new { CompletedStatus, MainFeedChannel })];

    return Result.Success(new ListLatestDraftsResponse
    {
      Drafts = results
    });
  }
}
