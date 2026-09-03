namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

// ── Query Handler ─────────────────────────────────────────────────────────

internal sealed class GetGuestDraftGameplayQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IUsersApi usersApi,
  IMovieTitleReader movieTitleReader
) : IQueryHandler<GetGuestDraftGameplayQuery, GetGuestDraftGameplayResponse>
{
  public async Task<Result<GetGuestDraftGameplayResponse>> Handle(
    GetGuestDraftGameplayQuery request,
    CancellationToken cancellationToken
  )
  {
    var caller = await usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure<GetGuestDraftGameplayResponse>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId)
      );
    }

    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string headerSql = $"""
      SELECT
        gd.public_id          AS {nameof(HeaderRow.GuestDraftPublicId)},
        gd.owner_user_id      AS {nameof(HeaderRow.OwnerUserId)},
        gd.title              AS {nameof(HeaderRow.Title)},
        gd.guest_draft_type   AS {nameof(HeaderRow.Type)},
        gd.guest_draft_status AS {nameof(HeaderRow.Status)},
        gd.share_token        AS {nameof(HeaderRow.ShareToken)}
      FROM guest_drafts.guest_drafts gd
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
      return Result.Failure<GetGuestDraftGameplayResponse>(
        GuestDraftErrors.NotFound(request.GuestDraftPublicId)
      );
    }

    // ── 2. Participants (with computed token balances) ──────────────────────
    const string participantSql = $"""
      SELECT
        gdp.id                            AS {nameof(ParticipantRow.Id)},
        gdp.public_id                     AS {nameof(ParticipantRow.PublicId)},
        gdp.user_id                       AS {nameof(ParticipantRow.UserId)},
        gdp.is_owner                      AS {nameof(ParticipantRow.IsOwner)},
        (gdp.starting_vetoes + gdp.awarded_vetoes - gdp.vetoes_used)
                                          AS {nameof(ParticipantRow.VetoTokensRemaining)},
        (gdp.awarded_veto_overrides - gdp.veto_overrides_used)
                                          AS {nameof(ParticipantRow.OverrideTokensRemaining)},
        (gdp.fungible_tokens + gdp.awarded_fungible_tokens - gdp.fungible_tokens_used)
                                          AS {nameof(ParticipantRow.FungibleTokensRemaining)},
        gdp.commissioner_overrides         AS {nameof(ParticipantRow.CommissionerOverridesUsed)}
      FROM guest_drafts.guest_draft_participants gdp
      JOIN guest_drafts.guest_drafts gd ON gd.id = gdp.guest_draft_id
      WHERE gd.public_id = @GuestDraftPublicId
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

    // ── 3. Positions ─────────────────────────────────────────────────────────
    // Picks is a genuine Postgres integer[] column (unlike canonical's
    // comma-delimited text column) -- Npgsql maps it straight to int[], no
    // manual ParsePicks(string) step needed.
    const string positionSql = $"""
      SELECT
        pos.public_id                    AS {nameof(PositionRow.PublicId)},
        pos.name                         AS {nameof(PositionRow.Name)},
        pos.picks                        AS {nameof(PositionRow.Picks)},
        pos.has_bonus_veto               AS {nameof(PositionRow.HasBonusVeto)},
        pos.has_bonus_veto_override      AS {nameof(PositionRow.HasBonusVetoOverride)},
        pos.has_bonus_fungible_token     AS {nameof(PositionRow.HasBonusFungibleToken)},
        pos.assigned_to_participant_id   AS {nameof(PositionRow.AssignedToParticipantId)}
      FROM guest_drafts.guest_draft_positions pos
      JOIN guest_drafts.guest_draft_game_boards gb ON gb.id = pos.game_board_id
      JOIN guest_drafts.guest_drafts gd ON gd.id = gb.guest_draft_id
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

    // ── 4. Picks (current veto/override/commissioner-override state) ───────
    const string pickSql = $"""
      SELECT
        pk.play_order                              AS {nameof(PickRow.PlayOrder)},
        pk.position                                AS {nameof(PickRow.Position)},
        pk.movie_public_id                         AS {nameof(PickRow.MoviePublicId)},
        pk.played_by_participant_id                AS {nameof(PickRow.PlayedByParticipantId)},
        pk.reveal_authorized_participant_id        AS {nameof(
        PickRow.RevealAuthorizedParticipantId
      )},
        pk.revealed_at                             AS {nameof(PickRow.RevealedAt)},
        (v.id IS NOT NULL AND v.is_overridden = FALSE)
                                                    AS {nameof(PickRow.WasVetoed)},
        (v.id IS NOT NULL AND v.is_overridden = TRUE)
                                                    AS {nameof(PickRow.WasVetoOverridden)},
        (co.id IS NOT NULL)                        AS {nameof(PickRow.WasCommissionerOverride)},
        v.issued_by_participant_id                 AS {nameof(PickRow.VetoedByParticipantId)},
        vo.issued_by_participant_id                AS {nameof(PickRow.SavedByParticipantId)},
        COALESCE(v.spent_from_fungible_pool, FALSE) AS {nameof(PickRow.WasVetoFungible)},
        COALESCE(vo.spent_from_fungible_pool, FALSE) AS {nameof(PickRow.WasVetoOverrideFungible)},
        COALESCE(v.sequence, 0)                    AS {nameof(PickRow.VetoSequence)}
      FROM guest_drafts.guest_draft_picks pk
      JOIN guest_drafts.guest_drafts gd ON gd.id = pk.guest_draft_id
      LEFT JOIN guest_drafts.guest_draft_vetoes v ON v.id = (
        SELECT v2.id FROM guest_drafts.guest_draft_vetoes v2
        WHERE v2.target_pick_id = pk.id
        ORDER BY v2.sequence DESC
        LIMIT 1
      )
      LEFT JOIN guest_drafts.guest_draft_commissioner_overrides co ON co.pick_id = pk.id
      LEFT JOIN guest_drafts.guest_draft_veto_overrides vo ON vo.veto_id = v.id
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

    // ── 5. Full veto history per pick ────────────────────────────────────────
    // Same split as canonical: the pick query above only carries the CURRENT veto
    // (drives board state); this pulls every veto ever issued against a pick, in
    // order, grouped by PlayOrder below into GameplayPickResponse.VetoHistory.
    const string vetoHistorySql = $"""
      SELECT
        pk.play_order                    AS {nameof(VetoHistoryRow.PlayOrder)},
        v.sequence                       AS {nameof(VetoHistoryRow.Sequence)},
        v.issued_by_participant_id       AS {nameof(VetoHistoryRow.VetoedByParticipantId)},
        v.spent_from_fungible_pool       AS {nameof(VetoHistoryRow.WasVetoFungible)},
        v.is_overridden                  AS {nameof(VetoHistoryRow.IsOverridden)},
        vo.issued_by_participant_id      AS {nameof(VetoHistoryRow.OverriddenByParticipantId)},
        COALESCE(vo.spent_from_fungible_pool, FALSE)
                                          AS {nameof(VetoHistoryRow.WasOverrideFungible)}
      FROM guest_drafts.guest_draft_vetoes v
      JOIN guest_drafts.guest_draft_picks pk ON pk.id = v.target_pick_id
      JOIN guest_drafts.guest_drafts gd ON gd.id = pk.guest_draft_id
      LEFT JOIN guest_drafts.guest_draft_veto_overrides vo ON vo.veto_id = v.id
      WHERE gd.public_id = @GuestDraftPublicId
      ORDER BY pk.play_order, v.sequence
      """;

    var vetoHistoryRows = (
      await connection.QueryAsync<VetoHistoryRow>(
        new CommandDefinition(
          vetoHistorySql,
          new { request.GuestDraftPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var vetoHistoryByPlayOrder = vetoHistoryRows
      .GroupBy(v => v.PlayOrder)
      .ToDictionary(g => g.Key, g => g.ToList());

    var users = await usersApi.GetUsersByIds(
      [.. participantRows.Select(p => p.UserId).Distinct()],
      cancellationToken
    );

    var displayNameByUserId = users.ToDictionary(
      u => u.UserId,
      u => $"{u.FirstName} {u.LastName}".Trim()
    );
    var participantIdToUserId = participantRows.ToDictionary(p => p.Id, p => p.UserId);
    var participantIdToPublicId = participantRows.ToDictionary(p => p.Id, p => p.PublicId);

    string? DisplayNameFor(Guid? participantId) =>
      participantId.HasValue
      && participantIdToUserId.TryGetValue(participantId.Value, out var userId)
        ? displayNameByUserId.GetValueOrDefault(userId)
        : null;

    string? PublicIdFor(Guid? participantId) =>
      participantId.HasValue
        ? participantIdToPublicId.GetValueOrDefault(participantId.Value)
        : null;

    // ── 7. Resolve movie titles ──────────────────────────────────────────────
    var movieTitles = await movieTitleReader.GetTitlesByPublicIdsAsync(
      pickRows.Select(p => p.MoviePublicId).Distinct(),
      cancellationToken
    );

    // ── 8. Caller context ────────────────────────────────────────────────────
    // No People/Drafters indirection needed here, unlike canonical -- a guest
    // draft participant's UserId is the caller's own resolved Guid directly.
    var isOwner = caller.UserId == header.OwnerUserId;
    var callerParticipant = participantRows.FirstOrDefault(p => p.UserId == caller.UserId);

    if (!isOwner && callerParticipant is null)
    {
      // Private by default, no share-token feature yet -- NotFound rather than
      // Forbidden, so a non-participant can't confirm a private draft even exists.
      return Result.Failure<GetGuestDraftGameplayResponse>(
        GuestDraftErrors.NotFound(request.GuestDraftPublicId)
      );
    }

    var callerContext = new CallerContextResponse
    {
      IsOwner = isOwner,
      IsParticipant = callerParticipant is not null,
      ParticipantPublicId = callerParticipant?.PublicId,
    };

    // ── 9. Assemble response ─────────────────────────────────────────────────
    return Result.Success(
      new GetGuestDraftGameplayResponse
      {
        GuestDraftPublicId = header.GuestDraftPublicId,
        Title = header.Title,
        Type = GuestDraftType.FromValue(header.Type).Name,
        Status = GuestDraftStatus.FromValue(header.Status).Name,
        ShareToken = isOwner ? header.ShareToken : null,
        CallerContext = callerContext,
        Positions =
        [
          .. positionRows.Select(pos => new GameplayPositionResponse
          {
            PositionPublicId = pos.PublicId,
            Name = pos.Name,
            Picks = pos.Picks,
            HasBonusVeto = pos.HasBonusVeto,
            HasBonusVetoOverride = pos.HasBonusVetoOverride,
            HasBonusFungibleToken = pos.HasBonusFungibleToken,
            AssignedParticipantPublicId = PublicIdFor(pos.AssignedToParticipantId),
            AssignedParticipantDisplayName = DisplayNameFor(pos.AssignedToParticipantId),
          }),
        ],
        Participants =
        [
          .. participantRows.Select(p => new GameplayParticipantResponse
          {
            ParticipantPublicId = p.PublicId,
            IsOwner = p.IsOwner,
            DisplayName = displayNameByUserId.GetValueOrDefault(p.UserId, "Unknown"),
            VetoTokensRemaining = p.VetoTokensRemaining,
            OverrideTokensRemaining = p.OverrideTokensRemaining,
            FungibleTokensRemaining = p.FungibleTokensRemaining,
            CommissionerOverridesUsed = p.CommissionerOverridesUsed,
          }),
        ],
        Picks =
        [
          .. pickRows.Select(p =>
          {
            var isRevealed = p.RevealedAt.HasValue;
            var callerIsPicker =
              callerParticipant is not null && p.PlayedByParticipantId == callerParticipant.Id;
            var callerIsRevealer =
              callerParticipant is not null
              && p.RevealAuthorizedParticipantId == callerParticipant.Id;
            var canSeeMovie = isRevealed || isOwner || callerIsPicker || callerIsRevealer;

            return new GameplayPickResponse
            {
              PlayOrder = p.PlayOrder,
              Position = p.Position,
              MoviePublicId = canSeeMovie ? p.MoviePublicId : null,
              MovieTitle = canSeeMovie ? movieTitles.GetValueOrDefault(p.MoviePublicId) : null,
              PlayedByParticipantPublicId = PublicIdFor(p.PlayedByParticipantId) ?? string.Empty,
              PlayedByDisplayName = DisplayNameFor(p.PlayedByParticipantId) ?? "Unknown",
              IsRevealed = isRevealed,
              WasVetoed = p.WasVetoed,
              WasVetoOverridden = p.WasVetoOverridden,
              WasCommissionerOverride = p.WasCommissionerOverride,
              IsActiveOnFinalBoard = !p.WasVetoed && !p.WasCommissionerOverride,
              IsEligibleForRePick = p.WasVetoed && !p.WasCommissionerOverride,
              VetoedByDisplayName = DisplayNameFor(p.VetoedByParticipantId),
              SavedByDisplayName = DisplayNameFor(p.SavedByParticipantId),
              WasVetoFungible = p.WasVetoFungible,
              WasVetoOverrideFungible = p.WasVetoOverrideFungible,
              VetoSequence = p.VetoSequence,
              RevealAuthorizedParticipantPublicId = PublicIdFor(p.RevealAuthorizedParticipantId),
              RevealAuthorizedByDisplayName = DisplayNameFor(p.RevealAuthorizedParticipantId),
              VetoHistory =
              [
                .. vetoHistoryByPlayOrder
                  .GetValueOrDefault(p.PlayOrder, [])
                  .Select(v => new GameplayVetoHistoryEntryResponse
                  {
                    Sequence = v.Sequence,
                    VetoedByDisplayName = DisplayNameFor(v.VetoedByParticipantId) ?? "Unknown",
                    WasVetoFungible = v.WasVetoFungible,
                    IsOverridden = v.IsOverridden,
                    OverriddenByDisplayName = DisplayNameFor(v.OverriddenByParticipantId),
                    WasOverrideFungible = v.WasOverrideFungible,
                  }),
              ],
            };
          }),
        ],
      }
    );
  }

  // ── Row types (positional records for Dapper) ────────────────────────────

  private sealed record HeaderRow(
    string GuestDraftPublicId,
    Guid OwnerUserId,
    string Title,
    int Type,
    int Status,
    string? ShareToken
  );

  private sealed record ParticipantRow(
    Guid Id,
    string PublicId,
    Guid UserId,
    bool IsOwner,
    int VetoTokensRemaining,
    int OverrideTokensRemaining,
    int FungibleTokensRemaining,
    int CommissionerOverridesUsed
  );

  private sealed record PositionRow
  {
    public string PublicId { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int[] Picks { get; init; } = default!;
    public bool HasBonusVeto { get; init; } = default!;
    public bool HasBonusVetoOverride { get; init; } = default!;
    public bool HasBonusFungibleToken { get; init; } = default!;
    public Guid? AssignedToParticipantId { get; init; } = Guid.Empty;
  }

  private sealed record PickRow
  {
    public int PlayOrder { get; init; } = default!;
    public int Position { get; init; } = default!;
    public string MoviePublicId { get; init; } = default!;
    public Guid PlayedByParticipantId { get; init; } = Guid.Empty;
    public Guid? RevealAuthorizedParticipantId { get; init; } = Guid.Empty;
    public DateTimeOffset? RevealedAt { get; init; } = default!;
    public bool WasVetoed { get; init; } = default!;
    public bool WasVetoOverridden { get; init; } = default!;
    public bool WasCommissionerOverride { get; init; } = default!;
    public Guid? VetoedByParticipantId { get; init; } = Guid.Empty;
    public Guid? SavedByParticipantId { get; init; } = Guid.Empty;
    public bool WasVetoFungible { get; init; } = default!;
    public bool WasVetoOverrideFungible { get; init; } = default!;
    public int VetoSequence { get; init; } = default!;
  }

  private sealed record VetoHistoryRow
  {
    public int PlayOrder { get; init; } = default!;
    public int Sequence { get; init; } = default!;
    public Guid VetoedByParticipantId { get; init; } = Guid.Empty;
    public bool WasVetoFungible { get; init; } = default!;
    public bool IsOverridden { get; init; } = default!;
    public Guid? OverriddenByParticipantId { get; init; } = Guid.Empty;
    public bool WasOverrideFungible { get; init; } = default!;
  }
}
