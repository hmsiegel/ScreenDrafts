namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.ListUpcomingDrafts;

internal sealed class ListUpcomingDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListUpcomingDraftsQuery, IReadOnlyList<UpcomingDraftDto>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyList<UpcomingDraftDto>>> Handle(ListUpcomingDraftsQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var sql = new StringBuilder(
      $"""
            SELECT
              d.id AS {nameof(UpcomingDraftDto.Id)},
              d.title AS {nameof(UpcomingDraftDto.Title)},
              d.draft_status AS {nameof(UpcomingDraftDto.DraftStatus)},
              COALESCE(
                array_agg(rd.release_date ORDER BY rd.release_date)
                FILTER (WHERE rd.release_date IS NOT NULL
                  AND rd.release_date > @Today),
                array[]::date[]
              ) AS {nameof(UpcomingDraftDto.ReleaseDates)}
            FROM drafts.drafts d
            LEFT JOIN drafts.draft_release_date rd ON d.id = rd.draft_id
            WHERE rd.release_date > @Today OR rd.release_date IS NULL
            """);

    if (!request.IsPatreonOnly)
    {
      sql.Append(" AND d.is_patreon_only = FALSE");
    }

    sql.Append(
      """
        GROUP BY d.id, d.title, d.draft_status
        ORDER BY MIN(rd.release_date) NULLS LAST;
      """
      );

    var drafts = (await connection.QueryAsync<UpcomingDraftDto>(sql.ToString(), new { DateTime.Today })).ToList();

    if (drafts.Count == 0)
    {
      return drafts;
    }

    var userId = request.UserId;
    var hostDraftIds = await connection.QueryAsync<Guid>(
      """
        SELECT 
          dh.draft_id
        FROM drafts.draft_hosts dh
        JOIN drafts.hosts h ON h.id = dh.host_id
        JOIN drafts.people p ON p.id = h.person_id
        WHERE p.user_id = @userId
      """,
      new { userId });

    var drafterDraftIds = await connection.QueryAsync<Guid>(
      """
        SELECT dd.draft_id
        FROM drafts.drafts_drafters dd
        JOIN drafts.drafters d ON d.id = dd.drafter_id
        JOIN drafts.people p ON p.id = d.person_id
        WHERE p.user_id = @userId
      """,
      new { userId });

    var isAdmin = request.IsAdmin;

    foreach (var draft in drafts)
    {
      var capabilities = new DraftUserCapabilities(
        Role: null,
        CanEdit: false,
        CanDelete: false,
        CanStart: false,
        CanPlay: false);

      if (isAdmin)
      {
        capabilities = capabilities with
        {
          Role = Roles.Admin,
          CanEdit = true,
          CanDelete = true,
          CanStart = true
        };
      }
      else if (hostDraftIds.Contains(draft.Id))
      {

        capabilities = capabilities with
        {
          Role = Roles.Commissioner,
          CanEdit = true,
          CanStart = true
        };
      }
      else if (drafterDraftIds.Contains(draft.Id))
      {
        capabilities = capabilities with
        {
          Role = Roles.Drafter,
          CanPlay = true
        };
      }

      draft.SetCapabilities(
        role: capabilities.Role,
        canEdit: capabilities.CanEdit,
        canDelete: capabilities.CanDelete,
        canStart: capabilities.CanStart,
        canPlay: capabilities.CanPlay);

    }

    return drafts;
  }
}
