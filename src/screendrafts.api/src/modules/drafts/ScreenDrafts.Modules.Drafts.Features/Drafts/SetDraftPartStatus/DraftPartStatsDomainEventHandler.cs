namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

/// <summary>
/// Second handler on DraftPartCompletedDomainEvent. Publishes the full stats facts for
/// Reporting's Record Book. Kept separate from DraftPartCompletedDomainEventHandler so the
/// existing summary event is untouched and a failure here never blocks it.
/// </summary>
internal sealed partial class DraftPartStatsDomainEventHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider,
  ILogger<DraftPartStatsDomainEventHandler> logger
) : DomainEventHandler<DraftPartCompletedDomainEvent>
{
  private const int DrafterKind = 0;
  private const int TeamKind = 1;

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly ILogger<DraftPartStatsDomainEventHandler> _logger = logger;

  public override async Task Handle(
    DraftPartCompletedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string headerSql = """
      SELECT
        d.public_id          AS DraftPublicId,
        d.title              AS DraftTitle,
        dp.public_id         AS DraftPartPublicId,
        dp.part_index        AS PartIndex,
        dp.draft_type        AS DraftType,
        s.name               AS SeriesName,
        s.canonical_policy   AS CanonicalPolicy,
        EXISTS (
          SELECT 1 FROM drafts.draft_releases dr
          WHERE dr.part_id = dp.id AND dr.release_channel = 0
        )                    AS HasMainFeedRelease
      FROM drafts.drafts d
      JOIN drafts.series s ON s.id = d.series_id
      JOIN drafts.draft_parts dp ON dp.id = @DraftPartId AND dp.draft_id = d.id
      WHERE d.id = @DraftId
      """;

    var header = await connection.QuerySingleOrDefaultAsync<HeaderRow>(
      new CommandDefinition(
        headerSql,
        new { domainEvent.DraftId, domainEvent.DraftPartId },
        cancellationToken: cancellationToken
      )
    );

    if (header is null)
    {
      LogHeaderNotFound(_logger, domainEvent.DraftPartId, domainEvent.DraftId);
      return;
    }

    const string picksSql = """
      SELECT
        p.id                                  AS Id,
        p.position                            AS Position,
        sd.index                              AS SubDraftIndex,
        p.movie_id                            AS MovieId,
        p.played_by_participant_kind_value    AS PlayedByKind,
        p.played_by_participant_id_value      AS PlayedByIdValue,
        EXISTS (
          SELECT 1 FROM drafts.commissioner_overrides co WHERE co.pick_id = p.id
        )                                     AS IsCommissionerOverridden
      FROM drafts.picks p
      LEFT JOIN drafts.sub_drafts sd ON sd.id = p.sub_draft_id
      WHERE p.draft_part_id = @DraftPartId
      ORDER BY sd.index NULLS FIRST, p.position
      """;

    const string vetoesSql = """
      SELECT
        v.id                          AS Id,
        v.target_pick_id              AS PickId,
        v.sequence                    AS Sequence,
        v.is_overridden               AS IsOverridden,
        ip.participant_kind_value     AS IssuedByKind,
        ip.participant_id_value       AS IssuedByIdValue,
        op.participant_kind_value     AS OverriddenByKind,
        op.participant_id_value       AS OverriddenByIdValue
      FROM drafts.vetoes v
      JOIN drafts.picks p ON p.id = v.target_pick_id
      JOIN drafts.draft_part_participants ip ON ip.id = v.issued_by_participant_id
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      LEFT JOIN drafts.draft_part_participants op ON op.id = vo.issued_by_participant_id
      WHERE p.draft_part_id = @DraftPartId
      """;

    const string creditsSql = """
      SELECT
        c.target_pick_id     AS PickId,
        c.drafter_id_value   AS DrafterIdValue
      FROM drafts.team_pick_credits c
      JOIN drafts.picks p ON p.id = c.target_pick_id
      WHERE p.draft_part_id = @DraftPartId
      """;

    var parameters = new { domainEvent.DraftPartId };

    var pickRows = (
      await connection.QueryAsync<PickRow>(
        new CommandDefinition(picksSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var vetoRows = (
      await connection.QueryAsync<VetoRow>(
        new CommandDefinition(vetoesSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var creditRows = (
      await connection.QueryAsync<CreditRow>(
        new CommandDefinition(creditsSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    // Media
    var movieIds = pickRows.Select(p => p.MovieId).Distinct().ToArray();
    var movieMap = new Dictionary<Guid, MovieRow>();

    if (movieIds.Length > 0)
    {
      const string moviesSql = """
        SELECT
          m.id           AS Id,
          m.public_id    AS PublicId,
          m.movie_title  AS Title
        FROM drafts.movies m
        WHERE m.id = ANY(@MovieIds)
        """;

      var movieRows = await connection.QueryAsync<MovieRow>(
        new CommandDefinition(
          moviesSql,
          new { MovieIds = movieIds },
          cancellationToken: cancellationToken
        )
      );

      movieMap = movieRows.ToDictionary(m => m.Id);
    }

    // Participants: drafters (players, issuers, override issuers, credits) and teams
    var drafterIds = pickRows
      .Where(p => p.PlayedByKind == DrafterKind)
      .Select(p => p.PlayedByIdValue)
      .Concat(vetoRows.Where(v => v.IssuedByKind == DrafterKind).Select(v => v.IssuedByIdValue))
      .Concat(
        vetoRows
          .Where(v => v.OverriddenByKind == DrafterKind && v.OverriddenByIdValue.HasValue)
          .Select(v => v.OverriddenByIdValue!.Value)
      )
      .Concat(creditRows.Select(c => c.DrafterIdValue))
      .Distinct()
      .ToArray();

    var teamIds = pickRows
      .Where(p => p.PlayedByKind == TeamKind)
      .Select(p => p.PlayedByIdValue)
      .Concat(vetoRows.Where(v => v.IssuedByKind == TeamKind).Select(v => v.IssuedByIdValue))
      .Concat(
        vetoRows
          .Where(v => v.OverriddenByKind == TeamKind && v.OverriddenByIdValue.HasValue)
          .Select(v => v.OverriddenByIdValue!.Value)
      )
      .Distinct()
      .ToArray();

    var drafterMap = new Dictionary<Guid, NamedRow>();
    var teamMap = new Dictionary<Guid, NamedRow>();

    if (drafterIds.Length > 0)
    {
      const string draftersSql = """
        SELECT
          dr.id          AS Id,
          dr.public_id   AS PublicId,
          COALESCE(
            NULLIF(pe.display_name, ''),
            NULLIF(TRIM(CONCAT_WS(' ', pe.first_name, pe.last_name)), ''),
            'Unknown drafter'
          )              AS Name
        FROM drafts.drafters dr
        JOIN drafts.people pe ON pe.id = dr.person_id
        WHERE dr.id = ANY(@Ids)
        """;

      var rows = await connection.QueryAsync<NamedRow>(
        new CommandDefinition(
          draftersSql,
          new { Ids = drafterIds },
          cancellationToken: cancellationToken
        )
      );

      drafterMap = rows.ToDictionary(r => r.Id);
    }

    if (teamIds.Length > 0)
    {
      const string teamsSql = """
        SELECT
          t.id          AS Id,
          t.public_id   AS PublicId,
          t.name        AS Name
        FROM drafts.drafter_teams t
        WHERE t.id = ANY(@Ids)
        """;

      var rows = await connection.QueryAsync<NamedRow>(
        new CommandDefinition(teamsSql, new { Ids = teamIds }, cancellationToken: cancellationToken)
      );

      teamMap = rows.ToDictionary(r => r.Id);
    }

    (string? PublicId, string Name) Resolve(int kind, Guid id) =>
      kind switch
      {
        DrafterKind => drafterMap.TryGetValue(id, out var d)
          ? (d.PublicId, d.Name)
          : (null, "Unknown drafter"),
        TeamKind => teamMap.TryGetValue(id, out var t)
          ? (t.PublicId, t.Name)
          : (null, "Unknown team"),
        _ => (null, "Community"),
      };

    var vetoesByPick = vetoRows.ToLookup(v => v.PickId);
    var creditsByPick = creditRows.ToLookup(c => c.PickId);

    var picks = new List<StatsPickRecord>(pickRows.Count);
    var skipped = 0;

    foreach (var pick in pickRows)
    {
      if (!movieMap.TryGetValue(pick.MovieId, out var media))
      {
        skipped++;
        continue;
      }

      var player = Resolve(pick.PlayedByKind, pick.PlayedByIdValue);

      var vetoes = vetoesByPick[pick.Id]
        .OrderBy(v => v.Sequence)
        .Select(v =>
        {
          var issuer = Resolve(v.IssuedByKind, v.IssuedByIdValue);
          (string? PublicId, string Name)? overrider =
            v.OverriddenByKind is { } kind && v.OverriddenByIdValue is { } overriderId
              ? Resolve(kind, overriderId)
              : null;

          return new StatsVetoRecord(
            VetoId: v.Id,
            Sequence: v.Sequence,
            IssuedByKind: v.IssuedByKind,
            IssuedByIdValue: v.IssuedByIdValue,
            IssuedByPublicId: issuer.PublicId,
            IssuedByName: issuer.Name,
            IsOverridden: v.IsOverridden,
            OverriddenByKind: v.OverriddenByKind,
            OverriddenByIdValue: v.OverriddenByIdValue,
            OverriddenByPublicId: overrider?.PublicId,
            OverriddenByName: overrider?.Name
          );
        })
        .ToList();

      IReadOnlyList<StatsCreditRecord> credits = pick.PlayedByKind switch
      {
        DrafterKind when drafterMap.TryGetValue(pick.PlayedByIdValue, out var solo) =>
        [
          new StatsCreditRecord(solo.Id, solo.PublicId, solo.Name),
        ],
        TeamKind =>
        [
          .. creditsByPick[pick.Id]
            .Where(c => drafterMap.ContainsKey(c.DrafterIdValue))
            .Select(c =>
            {
              var member = drafterMap[c.DrafterIdValue];
              return new StatsCreditRecord(member.Id, member.PublicId, member.Name);
            }),
        ],
        _ => [],
      };

      picks.Add(
        new StatsPickRecord(
          PickId: pick.Id,
          Position: pick.Position,
          SubDraftIndex: pick.SubDraftIndex,
          MediaPublicId: media.PublicId,
          MediaTitle: media.Title,
          PlayedByKind: pick.PlayedByKind,
          PlayedByIdValue: pick.PlayedByIdValue,
          PlayedByPublicId: player.PublicId,
          PlayedByName: player.Name,
          IsCommissionerOverridden: pick.IsCommissionerOverridden,
          Vetoes: vetoes,
          Credits: credits
        )
      );
    }

    if (skipped > 0)
    {
      LogPicksSkipped(_logger, skipped, domainEvent.DraftPartId);
    }

    await _eventBus.PublishAsync(
      new DraftPartStatsRecordedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftId: domainEvent.DraftId,
        draftPublicId: header.DraftPublicId,
        draftPartPublicId: header.DraftPartPublicId,
        partIndex: header.PartIndex,
        draftTitle: header.DraftTitle,
        draftType: DraftType.FromValue(header.DraftType).Name,
        seriesName: header.SeriesName,
        canonicalPolicyValue: header.CanonicalPolicy,
        hasMainFeedRelease: header.HasMainFeedRelease,
        picks: picks
      ),
      cancellationToken
    );
  }

  [LoggerMessage(
    10,
    LogLevel.Warning,
    "DraftPartStats — no draft row resolved for part {DraftPartId} / draft {DraftId}. "
      + "Facts were not published. Check that the draft has a non-null series_id."
  )]
  private static partial void LogHeaderNotFound(ILogger logger, Guid draftPartId, Guid draftId);

  [LoggerMessage(
    11,
    LogLevel.Warning,
    "DraftPartStats — skipped {Skipped} pick(s) on part {DraftPartId} whose media row was not found in drafts.movies."
  )]
  private static partial void LogPicksSkipped(ILogger logger, int skipped, Guid draftPartId);

  private sealed record HeaderRow(
    string DraftPublicId,
    string DraftTitle,
    string DraftPartPublicId,
    int PartIndex,
    int DraftType,
    string SeriesName,
    int CanonicalPolicy,
    bool HasMainFeedRelease
  );

  private sealed record PickRow(
    Guid Id,
    int Position,
    int? SubDraftIndex,
    Guid MovieId,
    int PlayedByKind,
    Guid PlayedByIdValue,
    bool IsCommissionerOverridden
  );

  private sealed record VetoRow(
    Guid Id,
    Guid PickId,
    int Sequence,
    bool IsOverridden,
    int IssuedByKind,
    Guid IssuedByIdValue,
    int? OverriddenByKind,
    Guid? OverriddenByIdValue
  );

  private sealed record CreditRow(Guid PickId, Guid DrafterIdValue);

  private sealed record MovieRow(Guid Id, string PublicId, string Title);

  private sealed record NamedRow(Guid Id, string PublicId, string Name);
}
