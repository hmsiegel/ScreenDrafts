namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.GetPickList;

internal sealed class GetPickListQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetPickListQuery, GetPickListResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetPickListResponse>> Handle(GetPickListQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string pickSql =
      $"""
      SELECT
        pk.id AS PickInternalId,
        pk.play_order AS {nameof(PickRow.PlayOrder)},
        pk.position AS {nameof(PickRow.Position)},
        m.imdb_id AS {nameof(PickRow.MovieImdbId)},
        m.movie_title AS {nameof(PickRow.MovieTitle)},
        pk.movie_version_name AS {nameof(PickRow.MovieVersionName)},
        pk.played_by_participant_id_value AS {nameof(PickRow.PlayedByParticipantIdValue)},
        pk.played_by_participant_kind_value AS {nameof(PickRow.PlayedByParticipantKindValue)},
        pk.acted_by_public_id AS {nameof(PickRow.ActedByPublicId)}
      FROM
        drafts.picks pk
        JOIN drafts.movies m ON pk.movie_id = m.id
        JOIN drafts.draft_parts dp ON pk.draft_part_id = dp.id
        WHERE dp.public_id = @DraftPartId
        ORDER BY pk.play_order ASC
      """;

    var pickRows = (await connection.QueryAsync<PickRow>(
      new CommandDefinition(
        pickSql,
        new { request.DraftPartId },
        cancellationToken: cancellationToken)))
      .ToList();

    if (pickRows.Count == 0)
    {
      return Result.Success(new GetPickListResponse
      {
        Picks = []
      });
    }

    var pickInternalIds = pickRows.Select(r => r.PickInternalId).ToArray();

    // 2. Vetoes
    const string vetoSql =
      $"""
      SELECT
        v.target_pick_id AS PickId,
        v.id AS VetoId,
        dpp.participant_id_value AS {nameof(VetoRow.IssuedByParticipantIdValue)},
        dpp.participant_kind_value AS {nameof(VetoRow.IssuedByParticipantKindValue)},
        v.acted_by_public_id AS {nameof(VetoRow.ActedByPublicId)},
        v.note AS {nameof(VetoRow.Note)},
        v.occurred_on AS {nameof(VetoRow.OccurredOntUtc)},
        v.is_overridden AS {nameof(VetoRow.IsOverridden)}
      FROM
        drafts.vetoes v
        JOIN drafts.draft_part_participants dpp ON v.issued_by_participant_id = dpp.id
      WHERE
        v.target_pick_id = ANY(@pickInternalIds)
      """;

    var vetoRows = (await connection.QueryAsync<VetoRow>(
      new CommandDefinition(
        vetoSql,
        new { pickInternalIds },
        cancellationToken: cancellationToken)))
      .ToList();

    var vetoIds = vetoRows.Select(r => r.VetoId).ToArray();

    // 3. Overrides
    const string overrideSql =
      $"""
      SELECT
        vo.veto_id AS VetoId,
        dpp.participant_id_value AS {nameof(VetoOverrideRow.IssuedByParticipantIdValue)},
        dpp.participant_kind_value AS {nameof(VetoOverrideRow.IssuedByParticipantKindValue)},
        vo.acted_by_public_id AS {nameof(VetoOverrideRow.ActedByPublicId)}
      FROM
        drafts.veto_overrides vo
        JOIN drafts.draft_part_participants dpp ON vo.issued_by_participant_id = dpp.id
      WHERE
        vo.veto_id = ANY(@vetoIds)
      """;

    var vetoOverrideByVetoId = vetoIds.Length > 0
      ? (await connection.QueryAsync<VetoOverrideRow>(
      new CommandDefinition(
        overrideSql,
        new { vetoIds },
        cancellationToken: cancellationToken)))
        .ToDictionary(r => r.VetoId)
      : [];

    // 4. Commissioner overrides
    const string commissionerOverrideSql =
      $"""
      SELECT
        co.pick_id AS PickId
      FROM
        drafts.commissioner_overrides co
      WHERE
        co.pick_id = ANY(@pickInternalIds)
      """;

    var commissionerOverridePickIds = pickInternalIds.Length > 0
      ? (await connection.QueryAsync<Guid>(
              new CommandDefinition(
                commissionerOverrideSql,
                new { pickInternalIds },
                cancellationToken: cancellationToken)))
      : [];

    // Assemble
    var vetoByPickId = vetoRows.ToDictionary(r => r.PickId);

    var picks = pickRows.Select(r =>
    {
      PickListVetoResponse? veto = null;
      if (vetoByPickId.TryGetValue(r.PickInternalId, out var vetoRow))
      {
        PickListVetoOverrideResponse? vetoOverride = null;
        if (vetoOverrideByVetoId.TryGetValue(vetoRow.VetoId, out var vo))
        {
          vetoOverride = new PickListVetoOverrideResponse
          {
            IssuedByParticipantIdValue = vo.IssuedByParticipantIdValue,
            IssuedByParticipantKindValue = vo.IssuedByParticipantKindValue,
            ActedByPublicId = vo.ActedByPublicId,
          };
        }

        veto = new PickListVetoResponse
        {
          IssuedByParticipantIdValue = vetoRow.IssuedByParticipantIdValue,
          IssuedByParticipantKindValue = vetoRow.IssuedByParticipantKindValue,
          ActedByPublicId = vetoRow.ActedByPublicId,
          Note = vetoRow.Note,
          OccurredOntUtc = vetoRow.OccurredOntUtc,
          IsOverridden = vetoRow.IsOverridden,
          Override = vetoOverride
        };
      }

      return new PickListItemResponse
      {
        PlayOrder = r.PlayOrder,
        Position = r.Position,
        MovieImdbId = r.MovieImdbId,
        MovieTitle = r.MovieTitle,
        MovieVersionName = r.MovieVersionName,
        PlayedByParticipantIdValue = r.PlayedByParticipantIdValue,
        PlayedByParticipantKindValue = r.PlayedByParticipantKindValue,
        ActedByPublicId = r.ActedByPublicId,
        Veto = veto,
        HasCommissionerOverride = commissionerOverridePickIds.Contains(r.PickInternalId)
      };
    }).ToList().AsReadOnly();

    return Result.Success(new GetPickListResponse
    {
      Picks = picks
    });
  }

  private sealed record VetoOverrideRow(
    Guid VetoId,
    Guid IssuedByParticipantIdValue,
    int IssuedByParticipantKindValue,
    string? ActedByPublicId);


  private sealed record VetoRow(
    Guid PickId,
    Guid VetoId,
    Guid IssuedByParticipantIdValue,
    int IssuedByParticipantKindValue,
    string? ActedByPublicId,
    string? Note,
    DateTime OccurredOntUtc,
    bool IsOverridden);

  private sealed record PickRow(
    Guid PickInternalId,
    int PlayOrder,
    int Position,
    string MovieImdbId,
    string MovieTitle,
    string? MovieVersionName,
    Guid PlayedByParticipantIdValue,
    int PlayedByParticipantKindValue,
    string? ActedByPublicId);
}
