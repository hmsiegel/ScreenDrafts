namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Get;

internal sealed class GetGuestDraftDetailsQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IUsersApi usersApi
) : IQueryHandler<GetGuestDraftDetailsQuery, GuestDraftDetailResponse>
{
  public async Task<Result<GuestDraftDetailResponse>> Handle(
    GetGuestDraftDetailsQuery request,
    CancellationToken cancellationToken
  )
  {
    var caller = await usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure<GuestDraftDetailResponse>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId)
      );
    }

    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // ── 1. Header ────────────────────────────────────────────────────────────
    const string headerSql = $"""
      SELECT
        gd.public_id          AS {nameof(HeaderRow.GuestDraftPublicId)},
        gd.owner_user_id      AS {nameof(HeaderRow.OwnerUserId)},
        gd.title              AS {nameof(HeaderRow.Title)},
        gd.guest_draft_type   AS {nameof(HeaderRow.Type)},
        gd.guest_draft_status AS {nameof(HeaderRow.Status)},
        gd.draft_date         AS {nameof(HeaderRow.DraftDate)}
      FROM guest_drafts.drafts gd
      WHERE gd.public_id = @GuestDraftPublicId
      """;

    var header = await connection.QuerySingleOrDefaultAsync<HeaderRow>(
      new CommandDefinition(
        headerSql,
        new { request.GuestDraftPublicId },
        cancellationToken: cancellationToken
      )
    );

    if (header is null)
    {
      return Result.Failure<GuestDraftDetailResponse>(
        DraftErrors.NotFound(request.GuestDraftPublicId)
      );
    }

    // ── 2. Participants -- only what's needed to resolve display names for
    // positions/picks below. No token counts, no IsOwner flag on the row
    // itself (owner-ness is resolved once against the header, not per
    // participant) -- none of that applies once a draft is done. ───────────
    const string participantSql = $"""
      SELECT
        gdp.id                      AS {nameof(ParticipantRow.Id)},
        gd.first_name               AS {nameof(ParticipantRow.DrafterFirstName)},
        gd.last_name                AS {nameof(ParticipantRow.DrafterLastName)},
        gd.user_id                  AS {nameof(ParticipantRow.DrafterUserId)}
      FROM guest_drafts.draft_participants gdp
      JOIN guest_drafts.drafts gd2 ON gd2.id = gdp.draft_id
      LEFT JOIN guest_drafts.drafters gd
        ON gd.id = gdp.participant_id_value AND gdp.participant_kind_value = 0
      WHERE gd2.public_id = @GuestDraftPublicId
      """;

    var participantRows = (
      await connection.QueryAsync<ParticipantRow>(
        new CommandDefinition(
          participantSql,
          new { request.GuestDraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // Owner-or-participant gate -- same posture as the gameplay endpoint.
    // Flagged as a design choice, not a hard requirement: canonical's own
    // /drafts/[id] summary page (getDraftDetails) appears to have no auth
    // gate at all. Keeping this one authenticated + scoped, consistent with
    // every other GuestDrafts read -- relax it if a fully public summary
    // link is actually wanted.
    var isOwner = caller.UserId == header.OwnerUserId;
    var isParticipant = participantRows.Any(p => p.DrafterUserId == caller.UserId);

    if (!isOwner && !isParticipant)
    {
      return Result.Failure<GuestDraftDetailResponse>(
        DrafterErrors.NotFound(request.GuestDraftPublicId)
      );
    }

    // ── 3. Positions ─────────────────────────────────────────────────────────
    const string positionSql = $"""
      SELECT
        pos.name                         AS {nameof(PositionRow.Name)},
        pos.picks                        AS {nameof(PositionRow.Picks)},
        pos.assigned_to_participant_id   AS {nameof(PositionRow.AssignedToParticipantId)}
      FROM guest_drafts.draft_positions pos
      JOIN guest_drafts.game_boards gb ON gb.id = pos.game_board_id
      JOIN guest_drafts.drafts gd ON gd.id = gb.draft_id
      WHERE gd.public_id = @GuestDraftPublicId
      ORDER BY pos.name
      """;

    var positionRows = (
      await connection.QueryAsync<PositionRow>(
        new CommandDefinition(
          positionSql,
          new { request.GuestDraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // ── 4. Picks -- same JOIN to guest_drafts.movies as the gameplay query,
    // so MovieTitle/MovieYear/TmdbId come straight off the row, not through
    // movieTitles.GetValueOrDefault(...) (that lookup doesn't exist here,
    // and was a leftover bug in the gameplay handler's own assembly step —
    // worth checking whether that's already been fixed there). No
    // canSeeMovie/reveal gating -- everything's already revealed by the
    // time a draft is Completed. ─────────────────────────────────────────────
    const string pickSql = $"""
      SELECT
        pk.position                                AS {nameof(PickRow.Position)},
        pk.movie_public_id                         AS {nameof(PickRow.MoviePublicId)},
        m.movie_title                              AS {nameof(PickRow.MovieTitle)},
        m.year                                     AS {nameof(PickRow.MovieYear)},
        m.tmdb_id                                  AS {nameof(PickRow.TmdbId)},
        pk.played_by_participant_id                AS {nameof(PickRow.PlayedByParticipantId)},
        (v.id IS NOT NULL AND v.is_overridden = FALSE)
                                                    AS {nameof(PickRow.WasVetoed)},
        (v.id IS NOT NULL AND v.is_overridden = TRUE)
                                                    AS {nameof(PickRow.WasVetoOverridden)},
        (co.id IS NOT NULL)                        AS {nameof(PickRow.WasCommissionerOverride)},
        v.issued_by_participant_id                 AS {nameof(PickRow.VetoedByParticipantId)},
        vo.issued_by_participant_id                AS {nameof(PickRow.SavedByParticipantId)}
      FROM guest_drafts.picks pk
      JOIN guest_drafts.drafts gd ON gd.id = pk.guest_draft_id
      JOIN guest_drafts.movies m ON m.id = pk.movie_id
      LEFT JOIN guest_drafts.vetoes v ON v.id = (
        SELECT v2.id FROM guest_drafts.vetoes v2
        WHERE v2.target_pick_id = pk.id
        ORDER BY v2.sequence DESC
        LIMIT 1
      )
      LEFT JOIN guest_drafts.commissioner_overrides co ON co.pick_id = pk.id
      LEFT JOIN guest_drafts.veto_overrides vo ON vo.veto_id = v.id
      WHERE gd.public_id = @GuestDraftPublicId
      ORDER BY pk.play_order
      """;

    var pickRows = (
      await connection.QueryAsync<PickRow>(
        new CommandDefinition(
          pickSql,
          new { request.GuestDraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    // ── 5. Display-name resolution -- pure C# lookup, no extra query. ───────
    var participantIdToDisplayName = participantRows.ToDictionary(
      p => p.Id,
      p =>
        string.IsNullOrEmpty(p.DrafterFirstName)
          ? "Unknown"
          : $"{p.DrafterFirstName} {p.DrafterLastName}".Trim()
    );

    string? DisplayNameFor(Guid? participantId) =>
      participantId.HasValue
        ? participantIdToDisplayName.GetValueOrDefault(participantId.Value)
        : null;

    // ── 6. Assemble response ─────────────────────────────────────────────────
    return Result.Success(
      new GuestDraftDetailResponse
      {
        PublicId = header.GuestDraftPublicId,
        Title = header.Title,
        Type = DraftType.FromValue(header.Type).Name,
        Status = DraftStatus.FromValue(header.Status).Name,
        DraftDate = header.DraftDate,
        Positions =
        [
          .. positionRows.Select(pos => new GuestDraftDetailPositionResponse
          {
            Name = pos.Name,
            Picks = pos.Picks,
            AssignedParticipantDisplayName = DisplayNameFor(pos.AssignedToParticipantId),
          }),
        ],
        Picks =
        [
          .. pickRows.Select(p => new GuestDraftDetailPickResponse
          {
            Position = p.Position,
            MoviePublicId = p.MoviePublicId,
            MovieTitle = p.MovieTitle,
            MovieYear = p.MovieYear,
            TmdbId = p.TmdbId,
            PlayedByDisplayName = DisplayNameFor(p.PlayedByParticipantId) ?? "Unknown",
            WasVetoed = p.WasVetoed,
            WasVetoOverridden = p.WasVetoOverridden,
            WasCommissionerOverride = p.WasCommissionerOverride,
            IsActiveOnFinalBoard = !p.WasVetoed && !p.WasCommissionerOverride,
            VetoedByDisplayName = DisplayNameFor(p.VetoedByParticipantId),
            SavedByDisplayName = DisplayNameFor(p.SavedByParticipantId),
          }),
        ],
      }
    );
  }

  private sealed record HeaderRow(
    string GuestDraftPublicId,
    Guid OwnerUserId,
    string Title,
    int Type,
    int Status,
    DateOnly? DraftDate
  );

  private sealed record ParticipantRow(
    Guid Id,
    string? DrafterFirstName,
    string? DrafterLastName,
    Guid? DrafterUserId
  );

  private sealed record PositionRow
  {
    public string Name { get; init; } = default!;
    public int[] Picks { get; init; } = default!;
    public Guid? AssignedToParticipantId { get; init; } = default!;
  }

  private sealed record PickRow
  {
    public int Position { get; init; } = default!;
    public string? MoviePublicId { get; init; } = default!;
    public string? MovieTitle { get; init; } = default!;
    public string? MovieYear { get; init; } = default!;
    public int? TmdbId { get; init; } = default!;
    public Guid PlayedByParticipantId { get; init; } = Guid.Empty;
    public bool WasVetoed { get; init; } = default!;
    public bool WasVetoOverridden { get; init; } = default!;
    public bool WasCommissionerOverride { get; init; } = default!;
    public Guid? VetoedByParticipantId { get; init; } = default!;
    public Guid? SavedByParticipantId { get; init; } = default!;
  }
}
