namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class DraftPartCompletedDomainEventHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<DraftPartCompletedDomainEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DraftPartCompletedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    // 1. Draft + part metadata
    const string draftSql = """
      SELECT
        d.public_id                AS DraftPublicId,
        d.title                   AS Title,
        dp.public_id               AS DraftPartPublicId,
        dp.part_index              AS PartIndex,
        dp.draft_type             AS DraftType,
        Cast((
          SELECT COUNT(*)
          FROM drafts.draft_parts
          WHERE draft_id = d.id
        ) AS INT4)                         AS TotalParts,
        EXISTS (
          SELECT 1
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id
            AND dcr.release_channel = 1
        )                         AS IsPatreon,
        (
          SELECT dcr.episode_number
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id
            AND dcr.release_channel = 0
        )                         AS EpisodeNumber
        FROM drafts.drafts d
        JOIN drafts.draft_parts dp ON dp.id = @DraftPartId
        WHERE d.id = @DraftId
      """;

    var draftRow = await connection.QuerySingleOrDefaultAsync<DraftRow>(
      new CommandDefinition(
        draftSql,
        new { domainEvent.DraftId, domainEvent.DraftPartId },
        cancellationToken: cancellationToken
      )
    );

    if (draftRow is null)
    {
      // Log and exit if the draft or part is not found
      // (This should not happen, but we want to avoid throwing exceptions in event handlers)
      return;
    }

    // 2. Active picks - not commissioner overridden, not vetoed

    const string activePicksSql = """
        SELECT
          p.position                        AS Position,
          p.movie_id                        AS MovieId
        FROM drafts.picks p
        WHERE p.draft_part_id = @DraftPartId
          AND NOT EXISTS (
            SELECT 1
            FROM drafts.commissioner_overrides co
            WHERE co.pick_id = p.id
          )
          AND NOT EXISTS (
            SELECT 1
            FROM drafts.vetoes v
            WHERE v.target_pick_id = p.id
            AND v.is_overridden = false
          )
        ORDER BY p.position
      """;

    var pickRows = (
      await connection.QueryAsync<PickRow>(
        new CommandDefinition(
          commandText: activePicksSql,
          parameters: new { domainEvent.DraftPartId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // 3. Vetoes deployed - vetoes that stuck (not overridden)
    const string vetoCountSql = """
      SELECT COUNT(*)
      FROM drafts.vetoes v
      JOIN drafts.picks p ON p.id = v.target_pick_id
      WHERE p.draft_part_id = @DraftPartId
        AND v.is_overridden = false
      """;

    var vetoCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        commandText: vetoCountSql,
        parameters: new { domainEvent.DraftPartId },
        cancellationToken: cancellationToken
      )
    );

    // 4. Resolve movie titles for picks
    var movieIds = pickRows.Select(pr => pr.MovieId).ToArray();
    var movieMap = new Dictionary<Guid, (string Title, string PublicId)>();

    if (movieIds.Length > 0)
    {
      const string moviesSql = """
          SELECT
            m.id           AS Id,
            m.public_id    AS PublicId,
            m.movie_title        AS Title
          FROM drafts.movies m
          WHERE m.id = ANY(@MovieIds)
        """;

      var movieRows = await connection.QueryAsync<MovieRow>(
        new CommandDefinition(
          commandText: moviesSql,
          parameters: new { MovieIds = movieIds },
          cancellationToken: cancellationToken
        )
      );

      movieMap = movieRows.ToDictionary(mr => mr.Id, mr => (mr.Title, mr.PublicId));
    }

    // 5. Participants and their kinds
    const string participantsSql = """
        SELECT
          p.participant_id_value            AS ParticipantId,
          p.participant_kind_value          AS Kind
        FROM drafts.draft_part_participants p
        WHERE p.draft_part_id = @DraftPartId
          AND p.participant_kind_value != 2
      """;

    var participantRows = await connection.QueryAsync<ParticipantRecord>(
      new CommandDefinition(
        commandText: participantsSql,
        parameters: new { domainEvent.DraftPartId },
        cancellationToken: cancellationToken
      )
    );

    //6. Resolve participant public ids - separate queries for drafters and teams
    var participntPublicIds = new List<string>();

    var drafterIds = participantRows
      .Where(pr => pr.Kind == 0)
      .Select(pr => pr.ParticipantId)
      .ToArray();

    var teamIds = participantRows
      .Where(pr => pr.Kind == 1)
      .Select(pr => pr.ParticipantId)
      .ToArray();

    if (drafterIds.Length > 0)
    {
      const string draftersSql = """
          SELECT
            d.public_id    AS PublicId
          FROM drafts.drafters d
          WHERE d.id = ANY(@DrafterIds)
        """;

      var drafterPublicIds = await connection.QueryAsync<string>(
        new CommandDefinition(
          commandText: draftersSql,
          parameters: new { DrafterIds = drafterIds },
          cancellationToken: cancellationToken
        )
      );

      participntPublicIds.AddRange(drafterPublicIds);
    }

    if (teamIds.Length > 0)
    {
      const string teamsSql = """
          SELECT
            t.public_id    AS PublicId
          FROM drafts.drafter_teams t
          WHERE t.id = ANY(@TeamIds)
        """;

      var teamPublicIds = await connection.QueryAsync<string>(
        new CommandDefinition(
          commandText: teamsSql,
          parameters: new { TeamIds = teamIds },
          cancellationToken: cancellationToken
        )
      );

      participntPublicIds.AddRange(teamPublicIds);
    }

    // 7. Build pick DTOs
    var picks = pickRows
      .Where(p => movieMap.ContainsKey(p.MovieId))
      .Select(p => new CompletedPickRecord(
        Position: p.Position,
        MediaPublicId: movieMap[p.MovieId].PublicId,
        MediaTitle: movieMap[p.MovieId].Title
      ))
      .ToList();

    await _eventBus.PublishAsync(
      new DraftPartCompletedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftId: domainEvent.DraftId,
        draftPublicId: draftRow.DraftPublicId,
        draftPartPublicId: draftRow.DraftPartPublicId,
        title: draftRow.Title,
        draftType: DraftType.FromValue(draftRow.DraftType).Name,
        totalPicks: picks.Count,
        partIndex: draftRow.PartIndex,
        totalParts: draftRow.TotalParts,
        isPatreon: draftRow.IsPatreon,
        episodeNumber: draftRow.EpisodeNumber,
        picks: picks,
        vetoCount: vetoCount,
        participantPublicIds: participntPublicIds
      ),
      cancellationToken
    );
  }

  private sealed record DraftRow(
    string DraftPublicId,
    string Title,
    string DraftPartPublicId,
    int PartIndex,
    int DraftType,
    int TotalParts,
    bool IsPatreon,
    int? EpisodeNumber
  );

  private sealed record PickRow(int Position, Guid MovieId);

  private sealed record MovieRow(Guid Id, string PublicId, string Title);

  private sealed record ParticipantRecord(Guid ParticipantId, int Kind);
}
