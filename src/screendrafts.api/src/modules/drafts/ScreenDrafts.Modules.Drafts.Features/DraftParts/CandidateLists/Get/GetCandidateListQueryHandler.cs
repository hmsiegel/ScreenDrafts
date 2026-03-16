namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.Get;

internal sealed class GetCandidateListQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetCandidateListQuery, GetCandidateListResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetCandidateListResponse>> Handle(GetCandidateListQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string countSql =
      $"""
      SELECT COUNT(*)
      FROM drafts.candidate_list_entries c
      JOIN drafts.draft_parts dp ON c.draft_part_id = dp.id
      WHERE dp.public_id = @DraftPartId
      """;

    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        countSql,
        new { request.DraftPartId },
        cancellationToken: cancellationToken));

    const string sql =
      $"""
      SELECT
        c.id AS {nameof(EntryRow.EntryId)},
        c.tmdb_id AS {nameof(EntryRow.TmdbId)},
        m.movie_title AS {nameof(EntryRow.MovieTitle)},
        m.imdb_id AS {nameof(EntryRow.MovieImdbId)},
        c.added_by_public_id AS {nameof(EntryRow.AddedByPublicId)},
        c.notes AS {nameof(EntryRow.Notes)},
        c.created_on_utc AS {nameof(EntryRow.CreatedOnUtc)},
        c.is_pending AS {nameof(EntryRow.IsPending)}
      FROM drafts.candidate_list_entries c
      JOIN drafts.draft_parts dp ON c.draft_part_id = dp.id
      LEFT JOIN drafts.movies m ON c.movie_id = m.id
      WHERE dp.public_id = @DraftPartId
      ORDER BY c.created_on_utc ASC
      LIMIT @PageSize OFFSET @Offset
      """;

    var offset = (request.Page - 1) * request.PageSize;

    var rows  = await connection.QueryAsync<EntryRow>(
      new CommandDefinition(
        sql,
        new
        {
          request.DraftPartId,
          request.PageSize,
          Offset = offset
        },
        cancellationToken: cancellationToken));

    var response = new GetCandidateListResponse
    {
      Response = new PagedResult<CandidateListEntryResponse>
      {
        Items = [.. rows.Select(row => new CandidateListEntryResponse
        {
          EntryId = row.EntryId,
          TmdbId = row.TmdbId,
          MovieTitle = row.MovieTitle,
          MovieImdbId = row.MovieImdbId,
          AddedByPublicId = row.AddedByPublicId,
          Notes = row.Notes,
          CreatedOnUtc = row.CreatedOnUtc,
          IsPending = row.IsPending
        })],
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
      }
    };

    return Result.Success(response);
  }

  private sealed record EntryRow(
    Guid EntryId,
    int TmdbId,
    string? MovieTitle,
    string? MovieImdbId,
    string AddedByPublicId,
    string? Notes,
    DateTime CreatedOnUtc,
    bool IsPending);
}
