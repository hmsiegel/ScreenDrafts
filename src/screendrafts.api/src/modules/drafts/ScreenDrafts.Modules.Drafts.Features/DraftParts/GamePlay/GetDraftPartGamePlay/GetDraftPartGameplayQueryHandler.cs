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
        d.fungible_token_name           AS {nameof(HeaderRow.FungibleTokenName)},
        d.is_hostless                   AS {nameof(HeaderRow.IsHostless)},
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
        pos.has_bonus_fungible_token    AS {nameof(PositionRow.HasBonusFungibleToken)},
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
        dpp.veto_overrides_rolling_in   AS {nameof(ParticipantRow.VetoOverridesRollingIn)},
        (dpp.fungible_tokens
          + dpp.fungible_tokens_rolling_in
          + dpp.awarded_fungible_tokens
          - dpp.fungible_tokens_used)   AS {nameof(ParticipantRow.FungibleTokensRemaining)},
        dpp.fungible_tokens_rolling_in  AS {nameof(ParticipantRow.FungibleTokensRollingIn)}
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
    // sub_draft_id IS NULL here — same reasoning as the pick query below, this
    // stays scoped to the part-level trivia round. Sub-draft trivia belongs to
    // the per-sub-draft on-demand query, not this response.
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
        AND tr.sub_draft_id IS NULL
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
        m.imdb_id                       AS {nameof(PickRow.ImdbId)},
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
        END                             AS {nameof(PickRow.SavedByName)},
        COALESCE(v.spent_from_fungible_pool, FALSE)
                                        AS {nameof(PickRow.WasVetoFungible)},
        COALESCE(vo.spent_from_fungible_pool, FALSE)
                                        AS {nameof(PickRow.WasVetoOverrideFungible)},
        COALESCE(v.sequence, 0)         AS {nameof(PickRow.VetoSequence)},
        dpp_ra.participant_id_value     AS {nameof(PickRow.RevealAuthorizedParticipantIdValue)},
        CASE
          WHEN dpp_ra.id IS NULL THEN NULL
          ELSE COALESCE(pe_ra.first_name || ' ' || pe_ra.last_name, dr_ra.public_id)
        END                             AS {nameof(PickRow.RevealAuthorizedByName)}
      FROM drafts.picks pk
      JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
      JOIN drafts.draft_part_participants dpp ON dpp.id = pk.played_by_participant_id
      JOIN drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN drafts.people pe ON pe.id = (
        SELECT dr2.person_id FROM drafts.drafters dr2 WHERE dr2.id = dpp.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt ON dt.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 1
      LEFT JOIN drafts.vetoes v ON v.id = (
        SELECT v2.id FROM drafts.vetoes v2
        WHERE v2.target_pick_id = pk.id
        ORDER BY v2.sequence DESC
        LIMIT 1
      )
      LEFT JOIN drafts.commissioner_overrides co ON co.pick_id = pk.id
      -- VETOED BY: current veto's issuer
      LEFT JOIN drafts.draft_part_participants dpp_v ON dpp_v.id = v.issued_by_participant_id
      LEFT JOIN drafts.people pe_v ON pe_v.id = (
        SELECT dr_v.person_id FROM drafts.drafters dr_v WHERE dr_v.id = dpp_v.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt_v ON dt_v.id = dpp_v.participant_id_value
        AND dpp_v.participant_kind_value = 1
      -- SAVED BY: current veto's override issuer, if any
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      LEFT JOIN drafts.draft_part_participants dpp_vo ON dpp_vo.id = vo.issued_by_participant_id
      LEFT JOIN drafts.people pe_vo ON pe_vo.id = (
        SELECT dr_vo.person_id FROM drafts.drafters dr_vo WHERE dr_vo.id = dpp_vo.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt_vo ON dt_vo.id = dpp_vo.participant_id_value
        AND dpp_vo.participant_kind_value = 1
      -- REVEAL AUTHORIZATION: hostless-draft only (see Pick.RevealAuthorizedParticipant's
      -- remarks) — always a Drafter by construction, so no Team/Community branch is needed
      -- here unlike the joins above.
      LEFT JOIN drafts.draft_part_participants dpp_ra ON dpp_ra.id = pk.reveal_authorized_participant_id
      LEFT JOIN drafts.drafters dr_ra ON dr_ra.id = dpp_ra.participant_id_value
        AND dpp_ra.participant_kind_value = 0
      LEFT JOIN drafts.people pe_ra ON pe_ra.id = dr_ra.person_id
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

    // ── 5b. Full veto history per pick ─────────────────────────────────────────
    // The pick query above intentionally only pulls the CURRENT veto (LIMIT 1 by
    // sequence DESC) for WasVetoed/VetoedByName/etc. — that stays as-is, it drives
    // board state. This separate query pulls every veto ever issued against a pick
    // in this part, in order, so the wizard can display the full history (vetoed,
    // overridden, re-vetoed, ...) instead of only the latest entry. Grouped into
    // GameplayPickResponse.VetoHistory by PlayOrder below.
    const string vetoHistorySql = $"""
      SELECT
        pk.play_order                   AS {nameof(VetoHistoryRow.PlayOrder)},
        v.sequence                      AS {nameof(VetoHistoryRow.Sequence)},
        CASE
          WHEN dpp_v.participant_kind_value = 2 THEN 'Patreon Members'
          ELSE COALESCE(pe_v.first_name || ' ' || pe_v.last_name, dt_v.name)
        END                             AS {nameof(VetoHistoryRow.VetoedByName)},
        v.spent_from_fungible_pool      AS {nameof(VetoHistoryRow.WasVetoFungible)},
        v.is_overridden                 AS {nameof(VetoHistoryRow.IsOverridden)},
        CASE
          WHEN vo.id IS NULL THEN NULL
          WHEN dpp_vo.participant_kind_value = 2 THEN 'Patreon Members'
          ELSE COALESCE(pe_vo.first_name || ' ' || pe_vo.last_name, dt_vo.name)
        END                             AS {nameof(VetoHistoryRow.OverriddenByName)},
        COALESCE(vo.spent_from_fungible_pool, FALSE)
                                        AS {nameof(VetoHistoryRow.WasOverrideFungible)}
      FROM drafts.vetoes v
      JOIN drafts.picks pk ON pk.id = v.target_pick_id
      JOIN drafts.draft_parts dp ON dp.id = pk.draft_part_id
      JOIN drafts.draft_part_participants dpp_v ON dpp_v.id = v.issued_by_participant_id
      LEFT JOIN drafts.people pe_v ON pe_v.id = (
        SELECT dr_v.person_id FROM drafts.drafters dr_v WHERE dr_v.id = dpp_v.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt_v ON dt_v.id = dpp_v.participant_id_value
        AND dpp_v.participant_kind_value = 1
      LEFT JOIN drafts.veto_overrides vo ON vo.veto_id = v.id
      LEFT JOIN drafts.draft_part_participants dpp_vo ON dpp_vo.id = vo.issued_by_participant_id
      LEFT JOIN drafts.people pe_vo ON pe_vo.id = (
        SELECT dr_vo.person_id FROM drafts.drafters dr_vo WHERE dr_vo.id = dpp_vo.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt_vo ON dt_vo.id = dpp_vo.participant_id_value
        AND dpp_vo.participant_kind_value = 1
      WHERE dp.public_id = @DraftPartPublicId
        AND pk.sub_draft_id IS NULL
      ORDER BY pk.play_order, v.sequence
      """;

    var vetoHistoryRows = (
      await connection.QueryAsync<VetoHistoryRow>(
        new CommandDefinition(
          vetoHistorySql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var vetoHistoryByPlayOrder = vetoHistoryRows
      .GroupBy(v => v.PlayOrder)
      .ToDictionary(g => g.Key, g => g.ToList());

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

    // ── 6b. Sub-drafts (summary only) ───────────────────────────────────────
    // Empty result for non-SpeedDraft parts. Subject is filtered for
    // non-host roles further down, once callerRoles is known.
    const string subDraftSql = $"""
      SELECT
        sd.public_id                    AS {nameof(SubDraftRow.PublicId)},
        sd.index                        AS {nameof(SubDraftRow.Index)},
        sd.status                       AS {nameof(SubDraftRow.Status)},
        sd.subject_kind                 AS {nameof(SubDraftRow.SubjectKind)},
        sd.subject_name                 AS {nameof(SubDraftRow.SubjectName)},
        sd.subject_imdb_id               AS {nameof(SubDraftRow.SubjectImdbId)}
      FROM drafts.sub_drafts sd
      JOIN drafts.draft_parts dp ON dp.id = sd.draft_part_id
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY sd.index
      """;

    var subDraftRows = (
      await connection.QueryAsync<SubDraftRow>(
        new CommandDefinition(
          subDraftSql,
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

    var canSeePendingSubjects = callerRoles.IsPrimaryHost || callerRoles.IsCoHost;
    const int subDraftStatusPending = 0;

    // ── 7b. Compute next expected participant ──────────────────────────────────
    // Find the highest board slot that has no landed pick.
    // Landed = pick exists at that position where WasVetoed = false OR WasVetoOverridden = true.
    var landedPositions = pickRows
      .Where(p => !p.WasCommissionerOverride && (!p.WasVetoed || p.WasVetoOverridden))
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

    // ── 8b. Booster's Champion assignments (Legends Mega) ─────────────────────
    // Same table/join shape as GetDraftQueryHandler's admin-facing version, scoped to
    // this single draft part instead of ANY(@partIds).
    const string boostersChampionAssignmentSql = $"""
      SELECT
        bca.public_id     AS {nameof(BoostersChampionAssignmentRow.PublicId)},
        dr.public_id      AS {nameof(BoostersChampionAssignmentRow.AssignedDrafterPublicId)},
        COALESCE(pe.first_name || ' ' || pe.last_name, dr.public_id)
                          AS {nameof(BoostersChampionAssignmentRow.AssignedDrafterDisplayName)},
        bca.tmdb_id       AS {nameof(BoostersChampionAssignmentRow.TmdbId)},
        m.movie_title     AS {nameof(BoostersChampionAssignmentRow.Title)}
      FROM drafts.draft_part_boosters_champion_assignments bca
      JOIN drafts.draft_parts dp ON dp.id = bca.draft_part_id
      JOIN drafts.drafters dr ON dr.id = bca.assigned_drafter_id_value
      JOIN drafts.people pe ON pe.id = dr.person_id
      LEFT JOIN drafts.movies m ON m.tmdb_id = bca.tmdb_id
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY bca.public_id;
      """;

    var boostersChampionAssignmentRows = (
      await connection.QueryAsync<BoostersChampionAssignmentRow>(
        new CommandDefinition(
          boostersChampionAssignmentSql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

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
        FungibleTokenName = header.FungibleTokenName,
        IsHostless = header.IsHostless,
        BoostersChampionAssignments =
        [
          .. boostersChampionAssignmentRows.Select(
            bca => new GameplayBoostersChampionAssignmentResponse
            {
              PublicId = bca.PublicId,
              AssignedDrafterPublicId = bca.AssignedDrafterPublicId,
              AssignedDrafterDisplayName = bca.AssignedDrafterDisplayName,
              TmdbId = bca.TmdbId,
              Title = bca.Title,
            }
          ),
        ],
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
            HasBonusFungibleToken = pos.HasBonusFungibleToken,
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
            VetoesRollingIn = p.VetoesRollingIn,
            VetoOverridesRollingIn = p.VetoOverridesRollingIn,
            FungibleTokensRemaining = p.FungibleTokensRemaining,
            FungibleTokensRollingIn = p.FungibleTokensRollingIn,
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
            ImdbId = p.ImdbId,
            PlayedById = p.PlayedByIdValue,
            PlayedByKind = p.PlayedByKindValue,
            PlayedByName = p.PlayedByName,
            WasVetoed = p.WasVetoed,
            WasVetoOverridden = p.WasVetoOverridden,
            WasCommissionerOverride = p.WasCommissionerOverride,
            VetoedByName = p.VetoedByName,
            SavedByName = p.SavedByName,
            WasVetoFungible = p.WasVetoFungible,
            WasVetoOverrideFungible = p.WasVetoOverrideFungible,
            VetoSequence = p.VetoSequence,
            RevealAuthorizedParticipantId = p.RevealAuthorizedParticipantIdValue,
            RevealAuthorizedByName = p.RevealAuthorizedByName,
            VetoHistory =
            [
              .. vetoHistoryByPlayOrder
                .GetValueOrDefault(p.PlayOrder, [])
                .Select(v => new GameplayVetoHistoryEntryResponse
                {
                  Sequence = v.Sequence,
                  VetoedByName = v.VetoedByName,
                  WasVetoFungible = v.WasVetoFungible,
                  IsOverridden = v.IsOverridden,
                  OverriddenByName = v.OverriddenByName,
                  WasOverrideFungible = v.WasOverrideFungible,
                }),
            ],
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
        SubDrafts =
        [
          .. subDraftRows.Select(sd =>
          {
            var hideSubject = sd.Status == subDraftStatusPending && !canSeePendingSubjects;

            return new GameplaySubDraftSummaryResponse
            {
              PublicId = sd.PublicId,
              Index = sd.Index,
              Status = sd.Status,
              SubjectKind = hideSubject ? null : sd.SubjectKind,
              SubjectName = hideSubject ? null : sd.SubjectName,
              SubjectImdbId = hideSubject ? null : sd.SubjectImdbId,
            };
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
    string? FungibleTokenName,
    bool IsHostless,
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
    bool HasBonusFungibleToken,
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
    int VetoOverridesRollingIn,
    int FungibleTokensRemaining,
    int FungibleTokensRollingIn
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
    int? TmdbId,
    string? ImdbId,
    Guid PlayedByIdValue,
    int PlayedByKindValue,
    string PlayedByName,
    bool WasVetoed,
    bool WasVetoOverridden,
    bool WasCommissionerOverride,
    string? VetoedByName,
    string? SavedByName,
    bool WasVetoFungible,
    bool WasVetoOverrideFungible,
    int VetoSequence,
    Guid? RevealAuthorizedParticipantIdValue,
    string? RevealAuthorizedByName
  );

  private sealed record VetoHistoryRow(
    int PlayOrder,
    int Sequence,
    string VetoedByName,
    bool WasVetoFungible,
    bool IsOverridden,
    string? OverriddenByName,
    bool WasOverrideFungible
  );

  private sealed record BoostersChampionAssignmentRow(
    string PublicId,
    string AssignedDrafterPublicId,
    string AssignedDrafterDisplayName,
    int? TmdbId,
    string? Title
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

  private sealed record SubDraftRow(
    string PublicId,
    int Index,
    int Status,
    int? SubjectKind,
    string? SubjectName,
    string? SubjectImdbId
  );

  private static int[] ParsePicks(string picks) =>
    string.IsNullOrWhiteSpace(picks)
      ? []
      : [.. picks.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)];
}
