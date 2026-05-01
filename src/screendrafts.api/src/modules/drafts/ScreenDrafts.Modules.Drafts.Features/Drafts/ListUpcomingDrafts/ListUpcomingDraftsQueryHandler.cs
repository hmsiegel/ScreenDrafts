namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed class ListUpcomingDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListUpcomingDraftsQuery, ListUpcomingDraftsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  private const int CreatedStatus = 0;
  private const int InProgressStatus = 2;
  private const int MainFeedChannel = 0;
  private const int PatreonChannel = 1;


  public async Task<Result<ListUpcomingDraftsResponse>> Handle(ListUpcomingDraftsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseSql =
      $"""
      SELECT
        dp.public_id AS {nameof(UpcomingDraftResponse.DraftPartPublicId)},
        d.public_id AS {nameof(UpcomingDraftResponse.DraftPublicId)},
        d.title AS {nameof(UpcomingDraftResponse.Title)},
        dp.part_index AS {nameof(UpcomingDraftResponse.PartNumber)},
        (
          SELECT COUNT(*)
          FROM drafts.draft_parts dp2
          WHERE dp2.draft_id = d.id
        ) AS {nameof(UpcomingDraftResponse.TotalParts)},
        dp.status AS {nameof(UpcomingDraftResponse.Status)},
        MIN(r.release_date) AS {nameof(UpcomingDraftResponse.ReleaseDate)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON dp.draft_id = d.id
      LEFT JOIN drafts.draft_releases r ON r.part_id = dp.id
      WHERE dp.status IN (@CreatedStatus, @InProgressStatus)
      """;

    var sqlBuilder = new StringBuilder(baseSql);

    if (!request.IncludePatreon)
    {
      sqlBuilder.Append(
        CultureInfo.InvariantCulture,
        $"""

        AND NOT EXISTS (
          SELECT 1
          FROM drafts.draft_releases dr2
          WHERE dr2.part_id = dp.id
          AND dr2.release_channel = {PatreonChannel}
          AND NOT EXISTS (
            SELECT 1
            FROM drafts.draft_releases dr3
            WHERE dr3.part_id = dp.id
            AND dr3.release_channel = {MainFeedChannel}
          )
        )
        """);
    }

    sqlBuilder.Append(
      """
      
      GROUP BY dp.id, dp.public_id, dp.part_index, dp.status, d.id, d.public_id, d.title
      ORDER BY MIN(r.release_date) ASC NULLS LAST
      """);

    var draftParts = (await connection.QueryAsync<UpcomingDraftResponse>(
      new CommandDefinition(
        sqlBuilder.ToString(),
        new { CreatedStatus, InProgressStatus, PatreonChannel, MainFeedChannel },
        cancellationToken: cancellationToken))).ToList();

    if (draftParts.Count == 0)
    {
      return Result.Success(new ListUpcomingDraftsResponse
      {
        Drafts = []
      });
    }

    var userId = request.UserId;

    const string hostSql =
      $"""
      SELECT
        dp.public_id
      FROM drafts.draft_hosts dh
      JOIN drafts.draft_parts dp ON dp.id = dh.draft_part_id
      JOIN drafts.hosts h ON h.id = dh.host_id
      JOIN drafts.people p ON p.id = h.person_id
      WHERE p.user_id = @userId
      """;

    var hostPartIds = (await connection.QueryAsync<string>(
      new CommandDefinition(
        hostSql,
        new { userId },
        cancellationToken: cancellationToken))).ToHashSet();

    const string participantSql =
      $"""
      SELECT
        dp.public_id
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value AND dpp.participant_kind_value = 0
      JOIN drafts.people p ON p.id = dr.person_id
      WHERE p.user_id = @userId
      """;

    var participantPartIds = (await connection.QueryAsync<string>(
      new CommandDefinition(
        participantSql,
        new { userId },
        cancellationToken: cancellationToken))).ToHashSet();

    foreach (var part in draftParts)
    {
      part.SetCapabilities(BuildCapabilities(
        isAdmin: request.IsAdmin,
        isHost: hostPartIds.Contains(part.DraftPartPublicId),
        isParticipant: participantPartIds.Contains(part.DraftPartPublicId),
        status: part.Status));
    }

    return Result.Success(new ListUpcomingDraftsResponse
    {
      Drafts = draftParts
    });
  }

  private static DraftUserCapabilities BuildCapabilities(
    bool isAdmin,
    bool isHost,
    bool isParticipant,
    int status)
  {
    if (isAdmin)
    {
      return new DraftUserCapabilities(
        Role: DraftRoles.Admin,
        CanEdit: true,
        CanDelete: true,
        CanStart: true,
        CanUpdateBoard: true,
        CanJoin: true);
    }

    if (isHost)
    {
      return new DraftUserCapabilities(
        Role: DraftRoles.Commissioner,
        CanEdit: true,
        CanDelete: true,
        CanStart: true,
        CanUpdateBoard: false,
        CanJoin: true);
    }

    if (isParticipant)
    {
      return new DraftUserCapabilities(
        Role: DraftRoles.Drafter,
        CanEdit: false,
        CanDelete: false,
        CanStart: false,
        CanUpdateBoard: true,
        CanJoin: status == InProgressStatus);
    }

    return new DraftUserCapabilities(
      Role: null,
      CanEdit: false,
      CanDelete: false,
      CanStart: false,
      CanUpdateBoard: false,
      CanJoin: status == InProgressStatus);
  }
}
