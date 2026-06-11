namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ListSpotlightDrafts;

internal sealed class ListSpotlightDraftsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<ListSpotlightDraftsQuery, PagedResult<ListSpotlightDraftsResponse>>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<PagedResult<ListSpotlightDraftsResponse>>> Handle(
    ListSpotlightDraftsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        ds.public_id            AS PublicId,
        ds.draft_public_id      AS DraftPublicId,
        s.title                 AS Title,
        s.draft_type            AS DraftType,
        s.episode_number        AS EpisodeNumber,
        ds.spotlight_description AS SpotlightDescription,
        ds.spotify_url          AS SpotifyUrl,
        ds.is_active            AS IsActive,
        ds.is_pinned            AS IsPinned,
        ds.activated_at_utc     AS ActivatedAtUtc,
        ds.created_at_utc       AS CreatedAtUtc,
        Cast(COUNT(1) OVER() AS int4) AS TotalCount
      FROM reporting.draft_spotlights ds
      JOIN reporting.draft_summaries s
        ON ds.draft_public_id = s.draft_public_id
       AND s.part_index = 1
      WHERE (@ExcludeActive = FALSE OR ds.is_active = FALSE)
        AND (@Query IS NULL OR s.title ILIKE '%' || @Query || '%')
        AND (@DraftType IS NULL OR s.draft_type = @DraftType)
      ORDER BY ds.is_active DESC, ds.activated_at_utc DESC NULLS LAST, ds.created_at_utc DESC
      OFFSET @Offset ROWS
      FETCH NEXT @PageSize ROWS ONLY
      """;

    var offset = (request.Page - 1) * request.PageSize;

    var rows = (
      await connection.QueryAsync<SpotlightRow>(
        new CommandDefinition(
          commandText: sql,
          parameters: new
          {
            Offset = offset,
            request.PageSize,
            request.ExcludeActive,
            Query = string.IsNullOrWhiteSpace(request.Query) ? null : request.Query.Trim(),
            DraftType = string.IsNullOrWhiteSpace(request.DraftType)
              ? null
              : request.DraftType.Trim(),
          },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;

    var items = rows.Select(r => new ListSpotlightDraftsResponse
      {
        PublicId = r.PublicId,
        DraftPublicId = r.DraftPublicId,
        Title = r.Title,
        DraftType = r.DraftType,
        EpisodeNumber = r.EpisodeNumber,
        SpotlightDescription = r.SpotlightDescription,
        SpotifyUrl = r.SpotifyUrl != null ? new Uri(r.SpotifyUrl) : null,
        IsActive = r.IsActive,
        IsPinned = r.IsPinned,
        ActivatedAtUtc = r.ActivatedAtUtc,
        CreatedAtUtc = r.CreatedAtUtc,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new PagedResult<ListSpotlightDraftsResponse>
      {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize,
      }
    );
  }

  private sealed record SpotlightRow(
    string PublicId,
    string DraftPublicId,
    string Title,
    string DraftType,
    int? EpisodeNumber,
    string SpotlightDescription,
    string? SpotifyUrl,
    bool IsActive,
    bool IsPinned,
    DateTime? ActivatedAtUtc,
    DateTime CreatedAtUtc,
    int TotalCount
  );
}
