namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

// ── Query Handler ─────────────────────────────────────────────────────────────

internal sealed class GetDraftPartGameplayQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IOptions<DraftsOptions> options
) : IQueryHandler<GetDraftPartGameplayQuery, GetDraftPartGameplayResponse>
{
  private readonly DraftsOptions _options = options.Value;

  public async Task<Result<GetDraftPartGameplayResponse>> Handle(
    GetDraftPartGameplayQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // ── 1. Draft part header ──────────────────────────────────────────────────
    const string headerSql = $"""
      SELECT
        dp.public_id                    AS {nameof(HeaderRow.DraftPartPublicId)},
        d.public_id                     AS {nameof(HeaderRow.DraftPublicId)},
        d.title                         AS {nameof(HeaderRow.DraftTitle)},
        dp.draft_type                   AS {nameof(HeaderRow.DraftType)},
        dp.part_index                   AS {nameof(HeaderRow.PartIndex)},
        Cast((SELECT COUNT(*) FROM drafts.draft_parts x WHERE x.draft_id = d.id) AS int4)
                                        AS {nameof(HeaderRow.TotalParts)},
        EXISTS (SELECT 1 FROM drafts.draft_pools pl WHERE pl.draft_id = d.id)
                                        AS {nameof(HeaderRow.HasDraftPool)},
        EXISTS (
          SELECT 1 FROM drafts.draft_boards db
          JOIN drafts.draft_board_items dbi ON dbi.draft_board_id = db.id
          WHERE db.draft_id = d.id
          LIMIT 1)                      AS {nameof(HeaderRow.HasDraftBoard)},        
        EXISTS (
           SELECT 1 FROM drafts.candidate_list_entries cle
           WHERE cle.draft_part_id = dp.id
           LIMIT 1)                      AS {nameof(HeaderRow.HasCandidateList)}
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      WHERE dp.public_id = @DraftPartPublicId
      """;

    var header = await connection.QuerySingleOrDefaultAsync<HeaderRow>(
      new CommandDefinition(
        headerSql,
        new { request.DraftPartPublicId },
        cancellationToken: cancellationToken
      )
    );

    if (header is null)
    {
      return Result.Failure<GetDraftPartGameplayResponse>(
        DraftPartErrors.NotFound(request.DraftPartPublicId)
      );
    }

    // ── 2. Draft positions (game board) ───────────────────────────────────────
    const string positionSql = $"""
      SELECT
        pos.public_id                   AS {nameof(PositionRow.PublicId)},
        pos.name                        AS {nameof(PositionRow.Name)},
        pos.picks                       AS {nameof(PositionRow.Picks)},
        pos.has_bonus_veto              AS {nameof(PositionRow.HasBonusVeto)},
        pos.has_bonus_veto_override     AS {nameof(PositionRow.HasBonusVetoOverride)},
        pos.assigned_to_id              AS {nameof(PositionRow.AssignedToId)},
        pos.assigned_to_kind            AS {nameof(PositionRow.AssignedToKind)}
      FROM drafts.draft_positions pos
      JOIN drafts.game_boards gb ON gb.id = pos.game_board_id
      JOIN drafts.draft_parts dp ON dp.id = gb.draft_part_id
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY pos.name
      """;

    var positionRows = (
      await connection.QueryAsync<PositionRow>(
        new CommandDefinition(
          positionSql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // ── 3. Participants with token counts ─────────────────────────────────────
    const string participantSql = $"""
      SELECT
        dpp.participant_id_value        AS {nameof(ParticipantRow.ParticipantIdValue)},
        dpp.participant_kind_value      AS {nameof(ParticipantRow.ParticipantKindValue)},
        COALESCE(dr.public_id, dt.public_id) AS {nameof(ParticipantRow.ParticipantPublicId)},
        COALESCE(pe.first_name || ' ' || pe.last_name, dt.name)
                                        AS {nameof(ParticipantRow.Name)},
        (dpp.starting_vetoes
          + dpp.vetoes_rolling_in
          + dpp.awarded_vetoes
          - dpp.vetoes_used)            AS {nameof(ParticipantRow.VetoTokensRemaining)},
        (dpp.veto_overrides_rolling_in
          + dpp.awarded_veto_overrides
          - dpp.veto_overrides_used)    AS {nameof(ParticipantRow.OverrideTokensRemaining)},
        dpp.vetoes_rolling_in           AS {nameof(ParticipantRow.VetoesRollingIn)},
        dpp.veto_overrides_rolling_in   AS {nameof(ParticipantRow.VetoOverridesRollingIn)}
      FROM drafts.draft_part_participants dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      LEFT JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 0
      LEFT JOIN drafts.people pe ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt ON dt.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 1
      WHERE dp.public_id = @DraftPartPublicId
        AND dpp.participant_kind_value != 2
      """;

    var participantRows = (
      await connection.QueryAsync<ParticipantRow>(
        new CommandDefinition(
          participantSql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // ── 4. Trivia results ─────────────────────────────────────────────────────
    const string triviaSql = $"""
      SELECT
        tr.participant_id               AS {nameof(TriviaRow.ParticipantIdValue)},
        tr.participant_kind             AS {nameof(TriviaRow.ParticipantKindValue)},
        COALESCE(pe.first_name || ' ' || pe.last_name, dt.name)
                                        AS {nameof(TriviaRow.Name)},
        tr.questions_won                AS {nameof(TriviaRow.QuestionsWon)},
        tr.position                     AS {nameof(TriviaRow.Position)}
      FROM drafts.trivia_results tr
      JOIN drafts.draft_parts dp ON dp.id = tr.draft_part_id
      LEFT JOIN drafts.drafters dr ON dr.id = tr.participant_id
        AND tr.participant_kind = 0
      LEFT JOIN drafts.people pe ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt ON dt.id = tr.participant_id
        AND tr.participant_kind = 1
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY tr.position
      """;

    var triviaRows = (
      await connection.QueryAsync<TriviaRow>(
        new CommandDefinition(
          triviaSql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // ── 5. Picks with veto/override state ─────────────────────────────────────
    // A pick is vetoed if a veto row exists and is_overridden = false.
    // A pick is veto-overridden if a veto row exists and is_overridden = true.
    // A pick is a commissioner override if a commissioner_overrides row exists.
    const string pickSql = $"""
      SELECT
        pk.play_order                   AS {nameof(PickRow.PlayOrder)},
        pk.position                     AS {nameof(PickRow.BoardPosition)},
        m.movie_title                   AS {nameof(PickRow.MovieTitle)},
        m.year                          AS {nameof(PickRow.MovieYear)},
        m.tmdb_id                       AS {nameof(PickRow.TmdbId)},
        dpp.participant_id_value        AS {nameof(PickRow.PlayedByIdValue)},
        dpp.participant_kind_value      AS {nameof(PickRow.PlayedByKindValue)},
        CASE
          WHEN dpp.participant_kind_value = 2 THEN 'Patreon Members'
          ELSE COALESCE(pe.first_name || ' ' || pe.last_name, dt.name)
        END                             AS {nameof(PickRow.PlayedByName)},
        (v.id IS NOT NULL AND v.is_overridden = FALSE)
                                        AS {nameof(PickRow.WasVetoed)},
        (v.id IS NOT NULL AND v.is_overridden = TRUE)
                                        AS {nameof(PickRow.WasVetoOverridden)},
        (co.id IS NOT NULL)             AS {nameof(PickRow.WasCommissionerOverride)},
        CASE
          WHEN v.id IS NULL THEN NULL
          WHEN dpp_v.participant_kind_value = 2 THEN 'Patreon Members'
          ELSE COALESCE(pe_v.first_name || ' ' || pe_v.last_name, dt_v.name)
        END                             AS {nameof(PickRow.VetoedByName)},
        CASE
          WHEN vo.id IS NULL THEN NULL
          WHEN dpp_vo.participant_kind_value = 2 THEN 'Patreon Members'
          ELSE COALESCE(pe_vo.first_name || ' ' || pe_vo.last_name, dt_vo.name)
        END                             AS {nameof(PickRow.SavedByName)}
      FROM drafts.picks pk
      JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
      JOIN drafts.draft_part_participants dpp ON dpp.id = pk.played_by_participant_id
      JOIN drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN drafts.people pe ON pe.id = (
        SELECT dr2.person_id FROM drafts.drafters dr2 WHERE dr2.id = dpp.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt ON dt.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 1
      LEFT JOIN drafts.vetoes v ON v.target_pick_id = pk.id
      LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
      -- VETOED BY: veto issuer
      LEFT JOIN drafts.draft_part_participants dpp_v ON dpp_v.id = v.issued_by_participant_id
      LEFT JOIN drafts.people pe_v ON pe_v.id = (
        SELECT dr_v.person_id FROM drafts.drafters dr_v WHERE dr_v.id = dpp_v.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt_v ON dt_v.id = dpp_v.participant_id_value
        AND dpp_v.participant_kind_value = 1
      -- SAVED BY: veto-override issuer
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      LEFT JOIN drafts.draft_part_participants dpp_vo ON dpp_vo.id = vo.issued_by_participant_id
      LEFT JOIN drafts.people pe_vo ON pe_vo.id = (
        SELECT dr_vo.person_id FROM drafts.drafters dr_vo WHERE dr_vo.id = dpp_vo.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt_vo ON dt_vo.id = dpp_vo.participant_id_value
        AND dpp_vo.participant_kind_value = 1
      WHERE dp.public_id = @DraftPartPublicId
        AND pk.sub_draft_id IS NULL
      ORDER BY pk.play_order
      """;

    var pickRows = (
      await connection.QueryAsync<PickRow>(
        new CommandDefinition(
          pickSql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // ── 6. Hosts ──────────────────────────────────────────────────────────────
    const string hostSql = $"""
      SELECT
        h.public_id                     AS {nameof(HostRow.PublicId)},
        pe.first_name || ' ' || pe.last_name
                                        AS {nameof(HostRow.Name)},
        (dh.role = 0)                   AS {nameof(HostRow.IsPrimary)}
      FROM drafts.draft_hosts dh
      JOIN drafts.draft_parts dp ON dp.id = dh.draft_part_id
      JOIN drafts.hosts h ON h.id = dh.host_id
      JOIN drafts.people pe ON pe.id = h.person_id
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY dh.role DESC
      """;

    var hostRows = (
      await connection.QueryAsync<HostRow>(
        new CommandDefinition(
          hostSql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // Community Film Rules
    const string communityFilmRuleSql = $"""
      SELECT
        cfr.public_id                   AS {nameof(CommunityFilmRuleRow.PublicId)},
        cfr.rule_kind                   AS {nameof(CommunityFilmRuleRow.RuleKind)},
        cfr.target_slot                 AS {nameof(CommunityFilmRuleRow.TargetSlot)},
        cfr.tmdb_id                     AS {nameof(CommunityFilmRuleRow.TmdbId)},
        m.movie_title                   AS {nameof(CommunityFilmRuleRow.Title)},
        cfr.was_auto_veto_fired         AS {nameof(CommunityFilmRuleRow.WasAutoVetoFired)}
      FROM drafts.draft_part_community_film_rules cfr
      JOIN drafts.draft_parts dp ON dp.id = cfr.draft_part_id
      LEFT JOIN drafts.movies m ON m.tmdb_id = cfr.tmdb_id
      WHERE dp.public_id = @DraftPartPublicId
      """;

    var communityFilmRuleRows = (
      await connection.QueryAsync<CommunityFilmRuleRow>(
        new CommandDefinition(
          communityFilmRuleSql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // ── 7. Caller roles ───────────────────────────────────────────────────────
    // Resolve caller's internal person.id from their public_id (drafts.people).
    // Then check host and participant membership for this draft part.
    var callerRoles = new CurrentUserRolesResponse();
    string? callerParticipantId = null;

    if (request.CallerUserId.HasValue)
    {
      const string callerRoleSql = $"""
        SELECT
          pe.id                           AS {nameof(CallerRoleRow.PersonId)},
          pe.public_id                    AS {nameof(CallerRoleRow.PersonPublicId)},
          EXISTS (
            SELECT 1 FROM drafts.draft_hosts dh
            JOIN drafts.draft_parts dp2 ON dp2.id = dh.draft_part_id
            JOIN drafts.hosts h ON h.id = dh.host_id
            WHERE dp2.public_id = @DraftPartPublicId
              AND h.person_id = pe.id
              AND dh.role = 0
          )                               AS {nameof(CallerRoleRow.IsPrimaryHost)},
          EXISTS (
            SELECT 1 FROM drafts.draft_hosts dh
            JOIN drafts.draft_parts dp2 ON dp2.id = dh.draft_part_id
            JOIN drafts.hosts h ON h.id = dh.host_id
            WHERE dp2.public_id = @DraftPartPublicId
              AND h.person_id = pe.id
              AND dh.role != 0
          )                               AS {nameof(CallerRoleRow.IsCoHost)},
          EXISTS (
            SELECT 1 FROM drafts.draft_part_participants dpp
            JOIN drafts.draft_parts dp2 ON dp2.id = dpp.draft_part_id
            JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
              AND dpp.participant_kind_value = 0
            WHERE dp2.public_id = @DraftPartPublicId
              AND dr.person_id = pe.id
          )                               AS {nameof(CallerRoleRow.IsParticipant)},
        (
          SELECT dpp.participant_id_value::text
          FROM drafts.draft_part_participants dpp
          JOIN drafts.draft_parts dp2 ON dp2.id = dpp.draft_part_id
          JOIN drafts.drafters dr ON dr.id = dpp.participant_id_value
            AND dpp.participant_kind_value = 0
          WHERE dp2.public_id = @DraftPartPublicId
            AND dr.person_id = pe.id
          LIMIT 1
        )                               AS {nameof(CallerRoleRow.ParticipantIdValue)}
        FROM drafts.people pe
        WHERE pe.user_id = @CallerUserId
        """;

      var callerRow = await connection.QuerySingleOrDefaultAsync<CallerRoleRow>(
        new CommandDefinition(
          callerRoleSql,
          new { request.DraftPartPublicId, request.CallerUserId },
          cancellationToken: cancellationToken
        )
      );

      if (callerRow is not null)
      {
        var isCommissioner = _options.CommissionerPersonPublicIds.Contains(
          callerRow.PersonPublicId,
          StringComparer.OrdinalIgnoreCase
        );

        callerRoles = new CurrentUserRolesResponse
        {
          IsPrimaryHost = callerRow.IsPrimaryHost,
          IsCoHost = callerRow.IsCoHost,
          IsParticipant = callerRow.IsParticipant,
          IsCommissioner = isCommissioner,
        };

        callerParticipantId = callerRow.ParticipantIdValue;
      }
    }

    // ── 7. Compute next expected participant ──────────────────────────────────
    // Find the highest board slot that has no landed pick.
    // Landed = pick exists at that position where WasVetoed = false OR WasVetoOverridden = true.
    var landedPositions = pickRows
      .Where(p => !p.WasVetoed || p.WasVetoOverridden)
      .Select(p => p.BoardPosition)
      .ToHashSet();

    Guid? nextParticipantId = null;
    int? nextParticipantKind = null;

    if (positionRows.Count > 0)
    {
      // Find the highest unfilled slot and look up which position owns it
      var nextSlot = positionRows
        .Where(pos => pos.AssignedToId.HasValue) // only consider assigned positions
        .SelectMany(pos => ParsePicks(pos.Picks).Select(slot => (slot, pos)))
        .Where(x => !landedPositions.Contains(x.slot))
        .OrderByDescending(x => x.slot)
        .Select(x => x.pos)
        .FirstOrDefault();

      if (nextSlot is not null)
      {
        nextParticipantId = nextSlot.AssignedToId;
        nextParticipantKind = nextSlot.AssignedToKind;
      }
    }

    // ── 8. Resolve assigned participant names for positions ───────────────────
    // Build a lookup from the participant rows we already fetched
    var participantNameLookup = participantRows.ToDictionary(
      p => (p.ParticipantIdValue, p.ParticipantKindValue),
      p => (string?)p.Name
    );

    // ── 9. Assemble response ──────────────────────────────────────────────────
    return Result.Success(
      new GetDraftPartGameplayResponse
      {
        DraftPartId = header.DraftPartPublicId,
        DraftId = header.DraftPublicId,
        DraftTitle = header.DraftTitle,
        DraftType = DraftType.FromValue(header.DraftType).Name,
        PartIndex = header.PartIndex,
        IsMultiPart = header.TotalParts > 1,
        IsFinalPart = header.PartIndex == header.TotalParts,
        HasDraftPool = header.HasDraftPool,
        HasDraftBoard = header.HasDraftBoard,
        HasCandidateList = header.HasCandidateList,
        CurrentUserRoles = callerRoles,
        CallerParticipantId = callerParticipantId,
        TriviaResults =
        [
          .. triviaRows.Select(t => new GameplayTriviaResultResponse
          {
            ParticipantId = t.ParticipantIdValue,
            ParticipantKind = t.ParticipantKindValue,
            ParticipantName = t.Name,
            QuestionsWon = t.QuestionsWon,
            Position = t.Position,
          }),
        ],
        DraftPositions =
        [
          .. positionRows.Select(pos => new GameplayDraftPositionResponse
          {
            PositionPublicId = pos.PublicId,
            PositionName = pos.Name,
            OwnedBoardSlots = ParsePicks(pos.Picks),
            HasBonusVeto = pos.HasBonusVeto,
            HasBonusVetoOverride = pos.HasBonusVetoOverride,
            AssignedParticipantId = pos.AssignedToId,
            AssignedParticipantKind = pos.AssignedToKind.HasValue ? pos.AssignedToKind.Value : null,
            AssignedParticipantName = pos.AssignedToId.HasValue
              ? participantNameLookup.GetValueOrDefault(
                (pos.AssignedToId.Value, pos.AssignedToKind ?? 0),
                null
              )
              : null,
            IsCommunityPosition = pos.AssignedToKind == 2,
          }),
        ],
        NextExpectedParticipantId = nextParticipantId?.ToString(),
        NextExpectedParticipantKind = nextParticipantKind,
        Participants =
        [
          .. participantRows.Select(p => new GameplayParticipantResponse
          {
            ParticipantId = p.ParticipantIdValue,
            ParticipantPublicId = p.ParticipantPublicId,
            ParticipantKind = p.ParticipantKindValue,
            ParticipantName = p.Name,
            VetoTokensRemaining = p.VetoTokensRemaining,
            OverrideTokensRemaining = p.OverrideTokensRemaining,
          }),
        ],
        Picks =
        [
          .. pickRows.Select(p => new GameplayPickResponse
          {
            PlayOrder = p.PlayOrder,
            BoardPosition = p.BoardPosition,
            MovieTitle = p.MovieTitle,
            MovieYear = p.MovieYear,
            TmdbId = p.TmdbId,
            PlayedById = p.PlayedByIdValue,
            PlayedByKind = p.PlayedByKindValue,
            PlayedByName = p.PlayedByName,
            WasVetoed = p.WasVetoed,
            WasVetoOverridden = p.WasVetoOverridden,
            WasCommissionerOverride = p.WasCommissionerOverride,
            VetoedByName = p.VetoedByName,
            SavedByName = p.SavedByName,
          }),
        ],
        Hosts =
        [
          .. hostRows.Select(h => new GameplayHostResponse
          {
            HostPublicId = h.PublicId,
            HostName = h.Name,
            IsPrimary = h.IsPrimary,
          }),
        ],
        CommunityFilmRules =
        [
          .. communityFilmRuleRows.Select(cfr => new GameplayCommunityFilmRuleResponse
          {
            PublicId = cfr.PublicId,
            RuleKind = cfr.RuleKind,
            TargetSlot = cfr.TargetSlot,
            TmdbId = cfr.TmdbId,
            Title = cfr.Title,
            WasAutoVetoFired = cfr.WasAutoVetoFired,
          }),
        ],
      }
    );
  }

  // ── Row types (init-property records for Dapper) ──────────────────────────

  private sealed record HeaderRow(
    string DraftPartPublicId,
    string DraftPublicId,
    string DraftTitle,
    int DraftType,
    int PartIndex,
    int TotalParts,
    bool HasDraftPool,
    bool HasDraftBoard,
    bool HasCandidateList
  );

  private sealed record PositionRow(
    string PublicId,
    string Name,
    string Picks,
    bool HasBonusVeto,
    bool HasBonusVetoOverride,
    Guid? AssignedToId,
    int? AssignedToKind
  );

  private sealed record ParticipantRow(
    Guid ParticipantIdValue,
    int ParticipantKindValue,
    string? ParticipantPublicId,
    string Name,
    int VetoTokensRemaining,
    int OverrideTokensRemaining,
    int VetoesRollingIn,
    int VetoOverridesRollingIn
  );

  private sealed record TriviaRow(
    Guid ParticipantIdValue,
    int ParticipantKindValue,
    string Name,
    int QuestionsWon,
    int Position
  );

  private sealed record PickRow(
    int PlayOrder,
    int BoardPosition,
    string MovieTitle,
    string? MovieYear,
    int TmdbId,
    Guid PlayedByIdValue,
    int PlayedByKindValue,
    string PlayedByName,
    bool WasVetoed,
    bool WasVetoOverridden,
    bool WasCommissionerOverride,
    string? VetoedByName,
    string? SavedByName
  );

  private sealed record CallerRoleRow(
    Guid PersonId,
    string PersonPublicId,
    bool IsPrimaryHost,
    bool IsCoHost,
    bool IsParticipant,
    string? ParticipantIdValue
  );

  private sealed record CommunityFilmRuleRow(
    string PublicId,
    int RuleKind,
    int? TargetSlot,
    int? TmdbId,
    string? Title,
    bool WasAutoVetoFired
  );

  private sealed record HostRow(string PublicId, string Name, bool IsPrimary);

  private static int[] ParsePicks(string picks) =>
    string.IsNullOrWhiteSpace(picks)
      ? []
      : [.. picks.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)];
}
