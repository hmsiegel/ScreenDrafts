namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.GetSubDraftGameplay;

// ── Handler ───────────────────────────────────────────────────────────────────

internal sealed class GetSubDraftGameplayQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetSubDraftGameplayQuery, GetSubDraftGameplayResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetSubDraftGameplayResponse>> Handle(
    GetSubDraftGameplayQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string headerSql = $"""
      SELECT
        sd.public_id AS {nameof(HeaderRow.PublicId)},
        sd.index     AS {nameof(HeaderRow.Index)},
        sd.status    AS {nameof(HeaderRow.Status)}
      FROM drafts.sub_drafts sd
      JOIN drafts.draft_parts dp ON dp.id = sd.draft_part_id
      WHERE dp.public_id = @DraftPartPublicId
        AND sd.public_id = @SubDraftPublicId
      """;

    var header = await connection.QuerySingleOrDefaultAsync<HeaderRow>(
      new CommandDefinition(
        headerSql,
        new { request.DraftPartPublicId, request.SubDraftPublicId },
        cancellationToken: cancellationToken
      )
    );

    if (header is null)
    {
      return Result.Failure<GetSubDraftGameplayResponse>(
        SubDraftErrors.NotFound(request.SubDraftPublicId)
      );
    }

    // Sub-draft's own board — fixed A/B positions, created and assigned at
    // AddSubDraft time (see SubDraft.AssignFixedPositions). Same row shape
    // as GetDraftPartGameplayQueryHandler's position query, joined through
    // the sub-draft's own game_board instead of the DraftPart's.
    const string positionSql = $"""
      SELECT
        pos.public_id                   AS {nameof(PositionRow.PublicId)},
        pos.name                        AS {nameof(PositionRow.Name)},
        pos.picks                       AS {nameof(PositionRow.Picks)},
        pos.assigned_to_id              AS {nameof(PositionRow.AssignedToId)},
        pos.assigned_to_kind            AS {nameof(PositionRow.AssignedToKind)}
      FROM drafts.draft_positions pos
      JOIN drafts.game_boards gb ON gb.id = pos.game_board_id
      JOIN drafts.sub_drafts sd ON sd.id = gb.sub_draft_id
      WHERE sd.public_id = @SubDraftPublicId
      ORDER BY pos.name
      """;

    var positionRows = (
      await connection.QueryAsync<PositionRow>(
        new CommandDefinition(
          positionSql,
          new { request.SubDraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // Same pick shape/joins as GetDraftPartGameplayQueryHandler's pick query,
    // scoped to this sub_draft_id instead of excluding it. Kept in sync with
    // that query's veto/override/saved-by logic — if that one changes, this
    // one needs the same change.
    const string pickSql = $"""
      SELECT
        pk.play_order                   AS {nameof(PickRow.PlayOrder)},
        pk.position                     AS {nameof(PickRow.BoardPosition)},
        m.movie_title                   AS {nameof(PickRow.MovieTitle)},
        m.year                          AS {nameof(PickRow.MovieYear)},
        m.tmdb_id                       AS {nameof(PickRow.TmdbId)},
        m.imdb_id                       AS {nameof(PickRow.ImdbId)},
        m.igdb_id                       AS {nameof(PickRow.IgdbId)},
        m.media_type                     AS {nameof(PickRow.MediaType)},
        dpp.participant_id_value        AS {nameof(PickRow.PlayedByIdValue)},
        dpp.participant_kind_value      AS {nameof(PickRow.PlayedByKindValue)},
        COALESCE(pe.first_name || ' ' || pe.last_name, dt.name)
                                        AS {nameof(PickRow.PlayedByName)},
        (v.id IS NOT NULL AND v.is_overridden = FALSE)
                                        AS {nameof(PickRow.WasVetoed)},
        (v.id IS NOT NULL AND v.is_overridden = TRUE)
                                        AS {nameof(PickRow.WasVetoOverridden)}
      FROM drafts.picks pk
      JOIN drafts.sub_drafts sd ON sd.id = pk.sub_draft_id
      JOIN drafts.draft_part_participants dpp ON dpp.id = pk.played_by_participant_id
      JOIN drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN drafts.people pe ON pe.id = (
        SELECT dr2.person_id FROM drafts.drafters dr2 WHERE dr2.id = dpp.participant_id_value
      )
      LEFT JOIN drafts.drafter_teams dt ON dt.id = dpp.participant_id_value
        AND dpp.participant_kind_value = 1
      LEFT JOIN drafts.vetoes v ON v.target_pick_id = pk.id
      WHERE sd.public_id = @SubDraftPublicId
      ORDER BY pk.play_order
      """;

    var pickRows = (
      await connection.QueryAsync<PickRow>(
        new CommandDefinition(
          pickSql,
          new { request.SubDraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    const string triviaSql = $"""
      SELECT
        tr.participant_id               AS {nameof(TriviaRow.ParticipantIdValue)},
        tr.participant_kind             AS {nameof(TriviaRow.ParticipantKindValue)},
        COALESCE(pe.first_name || ' ' || pe.last_name, dt.name)
                                        AS {nameof(TriviaRow.Name)},
        tr.questions_won                AS {nameof(TriviaRow.QuestionsWon)},
        tr.position                     AS {nameof(TriviaRow.Position)}
      FROM drafts.trivia_results tr
      JOIN drafts.sub_drafts sd ON sd.id = tr.sub_draft_id
      LEFT JOIN drafts.drafters dr ON dr.id = tr.participant_id
        AND tr.participant_kind = 0
      LEFT JOIN drafts.people pe ON pe.id = dr.person_id
      LEFT JOIN drafts.drafter_teams dt ON dt.id = tr.participant_id
        AND tr.participant_kind = 1
      WHERE sd.public_id = @SubDraftPublicId
      ORDER BY tr.position
      """;

    var triviaRows = (
      await connection.QueryAsync<TriviaRow>(
        new CommandDefinition(
          triviaSql,
          new { request.SubDraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // Resolve names for assigned positions from the pick/trivia rows we
    // already have — cheap fallback since we didn't fetch the main
    // participants list here (only 2 participants, and one of these row
    // sets will usually have at least one name by the time a tab is active).
    string? ResolveName(Guid? id) =>
      pickRows.FirstOrDefault(p => p.PlayedByIdValue == id)?.PlayedByName
      ?? triviaRows.FirstOrDefault(t => t.ParticipantIdValue == id)?.Name;

    return Result.Success(
      new GetSubDraftGameplayResponse
      {
        SubDraftPublicId = header.PublicId,
        Index = header.Index,
        Status = header.Status,
        DraftPositions =
        [
          .. positionRows.Select(pos => new GameplayDraftPositionResponse
          {
            PositionPublicId = pos.PublicId,
            PositionName = pos.Name,
            OwnedBoardSlots = ParsePicks(pos.Picks),
            HasBonusVeto = false,
            HasBonusVetoOverride = false,
            AssignedParticipantId = pos.AssignedToId,
            AssignedParticipantKind = pos.AssignedToKind,
            AssignedParticipantName = ResolveName(pos.AssignedToId),
            IsCommunityPosition = false,
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
            IgdbId = p.IgdbId,
            MediaType = p.MediaType,
            PlayedById = p.PlayedByIdValue,
            PlayedByKind = p.PlayedByKindValue,
            PlayedByName = p.PlayedByName,
            WasVetoed = p.WasVetoed,
            WasVetoOverridden = p.WasVetoOverridden,
            // Sub-draft picks never have a commissioner override — that's
            // enforced at the domain level (ApplyVetoOverride blocks
            // SpeedDraft; commissioner overrides were never wired to
            // sub-drafts either).
            WasCommissionerOverride = false,
            VetoedByName = null,
            SavedByName = null,
          }),
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
      }
    );
  }

  private sealed record HeaderRow(string PublicId, int Index, int Status);

  private sealed record PositionRow(
    string PublicId,
    string Name,
    string Picks,
    Guid? AssignedToId,
    int? AssignedToKind
  );

  private sealed record PickRow(
    int PlayOrder,
    int BoardPosition,
    string MovieTitle,
    string? MovieYear,
    int? TmdbId,
    string? ImdbId,
    int? IgdbId,
    int? MediaType,
    Guid PlayedByIdValue,
    int PlayedByKindValue,
    string PlayedByName,
    bool WasVetoed,
    bool WasVetoOverridden
  );

  private sealed record TriviaRow(
    Guid ParticipantIdValue,
    int ParticipantKindValue,
    string Name,
    int QuestionsWon,
    int Position
  );

  private static int[] ParsePicks(string picks) =>
    string.IsNullOrWhiteSpace(picks)
      ? []
      : [.. picks.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)];
}
