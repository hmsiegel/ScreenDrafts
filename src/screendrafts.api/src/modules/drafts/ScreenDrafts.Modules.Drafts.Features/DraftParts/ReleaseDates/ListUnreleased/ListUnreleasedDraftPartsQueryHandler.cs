namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ReleaseDates.ListUnreleased;

internal sealed class ListUnreleasedDraftPartsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListUnreleasedDraftPartsQuery, PagedResult<UnreleasedDraftPartResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<PagedResult<UnreleasedDraftPartResponse>>> Handle(
    ListUnreleasedDraftPartsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // "Not released" = zero rows in drafts.draft_releases for this part, on
    // any channel — not the MainFeed-specific HasMainFeedRelease pattern
    // used by the canonical-picks seeder elsewhere. Different concern: a
    // Patreon-only part IS released for this purpose, it just isn't public yet.
    const string baseSql = $"""
      SELECT
        dp.public_id AS {nameof(UnreleasedDraftPartResponse.DraftPartPublicId)},
        d.public_id AS {nameof(UnreleasedDraftPartResponse.DraftPublicId)},
        d.title AS {nameof(UnreleasedDraftPartResponse.DraftTitle)},
        dp.part_index AS {nameof(UnreleasedDraftPartResponse.PartIndex)},
        d.draft_type AS {nameof(UnreleasedDraftPartResponse.DraftType)},
        s.public_id AS {nameof(UnreleasedDraftPartResponse.SeriesPublicId)},
        s.name AS {nameof(UnreleasedDraftPartResponse.SeriesName)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      JOIN drafts.series s ON s.id = d.series_id
      WHERE dp.status = @completedStatus
        AND d.is_deleted = false
        AND NOT EXISTS (
          SELECT 1 FROM drafts.draft_releases dr WHERE dr.part_id = dp.id
        )

      """;

    var sqlBuilder = new StringBuilder(baseSql);
    var parameters = new DynamicParameters();
    parameters.Add("completedStatus", DraftPartStatus.Completed.Value);

    if (!string.IsNullOrWhiteSpace(request.DraftPublicId))
    {
      sqlBuilder.Append(" AND d.public_id = @draftPublicId");
      parameters.Add("draftPublicId", request.DraftPublicId);
    }

    sqlBuilder.Append(" ORDER BY d.title ASC, dp.part_index ASC");

#pragma warning disable S2077
    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        $"SELECT COUNT(*) FROM ({sqlBuilder}) sub",
        parameters,
        cancellationToken: cancellationToken
      )
    );
#pragma warning restore S2077

    var pageSize = Math.Min(request.PageSize, 100);
    var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

    parameters.Add("pageSize", pageSize);
    parameters.Add("skip", skip);
    sqlBuilder.Append(" LIMIT @pageSize OFFSET @skip");

    var items = (
      await connection.QueryAsync<UnreleasedDraftPartResponse>(
        new CommandDefinition(
          sqlBuilder.ToString(),
          parameters,
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    return Result.Success(
      new PagedResult<UnreleasedDraftPartResponse>
      {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = pageSize,
      }
    );
  }
}
