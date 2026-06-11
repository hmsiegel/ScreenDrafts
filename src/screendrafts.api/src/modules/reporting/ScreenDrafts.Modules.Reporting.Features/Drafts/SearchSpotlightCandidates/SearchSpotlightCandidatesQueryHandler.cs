namespace ScreenDrafts.Modules.Reporting.Features.Drafts.SearchSpotlightCandidates;

internal sealed class SearchSpotlightCandidatesQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<SearchSpotlightCandidatesQuery, SearchSpotlightCandidatesResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<SearchSpotlightCandidatesResponse>> Handle(
    SearchSpotlightCandidatesQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    // One row per draft (part_index = 1 selects the canonical summary row).
    // Excludes Patreon drafts and incomplete drafts.
    // Excludes drafts that already have a spotlight record (any state).
    const string sql = """
      SELECT
        s.draft_public_id   AS DraftPublicId,
        s.title             AS Title,
        s.draft_type        AS DraftType,
        s.episode_number    AS EpisodeNumber,
        s.total_picks       AS TotalPicks,
        Cast(COUNT(1) OVER() AS int4) AS TotalCount
      FROM reporting.draft_summaries s
      WHERE s.part_index = 1
        AND s.is_complete = TRUE
        AND s.is_patreon  = FALSE
        AND NOT EXISTS (
          SELECT 1
          FROM reporting.draft_spotlights ds
          WHERE ds.draft_public_id = s.draft_public_id
        )
        AND (@Query IS NULL OR s.title ILIKE '%' || @Query || '%')
      ORDER BY s.episode_number DESC NULLS LAST
      OFFSET @Offset ROWS
      FETCH NEXT @PageSize ROWS ONLY
      """;

    var offset = (request.Page - 1) * request.PageSize;

    var rows = (
      await connection.QueryAsync<CandidateRow>(
        new CommandDefinition(
          commandText: sql,
          parameters: new
          {
            Query = string.IsNullOrWhiteSpace(request.Query) ? null : request.Query,
            Offset = offset,
            request.PageSize,
          },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

    var items = rows.Select(r => new SpotlightCandidateItem
      {
        DraftPublicId = r.DraftPublicId,
        Title = r.Title,
        DraftType = r.DraftType,
        EpisodeNumber = r.EpisodeNumber,
        TotalPicks = r.TotalPicks,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new SearchSpotlightCandidatesResponse
      {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize,
      }
    );
  }

  private sealed record CandidateRow(
    string DraftPublicId,
    string Title,
    string DraftType,
    int? EpisodeNumber,
    int TotalPicks,
    int TotalCount
  );
}
