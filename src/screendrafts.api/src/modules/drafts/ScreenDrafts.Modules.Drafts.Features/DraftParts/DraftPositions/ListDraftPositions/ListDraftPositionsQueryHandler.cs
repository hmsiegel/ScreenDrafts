namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

internal sealed class ListDraftPositionsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListDraftPositionsQuery, ListDraftPositionsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<ListDraftPositionsResponse>> Handle(ListDraftPositionsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string positionSql =
      $"""
      SELECT
        dp.public_id AS {nameof(PositionRow.PublicId)},
        dp.name AS {nameof(PositionRow.Name)},
        dp.picks AS {nameof(PositionRow.Picks)},
        dp.has_bonus_veto AS {nameof(PositionRow.HasBonusVeto)},
        dp.has_bonus_veto_override AS {nameof(PositionRow.HasBonusVetoOverride)},
        dp.assigned_to_id AS {nameof(PositionRow.AssignedTo_ParticipantId)},
        dp.assigned_to_kind AS {nameof(PositionRow.AssignedTo_ParticipantKind)}
      FROM
        drafts.draft_positions dp
      JOIN drafts.game_boards gb on dp.game_board_id = gb.id
      JOIN drafts.draft_parts dp2 on gb.draft_part_id = dp2.id
      WHERE
        dp2.public_id = @DraftPartId
      ORDER BY dp.name ASC
      """;

    var positionRows = (await connection.QueryAsync<PositionRow>(
      new CommandDefinition(
        commandText: positionSql,
        parameters: new { request.DraftPartId },
        cancellationToken: cancellationToken)))
      .ToList();

    if (positionRows.Count == 0)
    {
      return Result.Success(new ListDraftPositionsResponse
      {
        Positions = []
      });
    }

    var assignedIds = positionRows
      .Where(r => r.AssignedTo_ParticipantId.HasValue)
      .Select(r => r.AssignedTo_ParticipantId!.Value)
      .Distinct()
      .ToArray();

    Dictionary<Guid, string> nameById = [];

    if (assignedIds.Length > 0)
    {
      const string nameSql =
        $"""
        SELECT
          d.id AS {nameof(ParticipantNameRow.ParticipantId)},
          p.display_name AS {nameof(ParticipantNameRow.ParticipantName)}
        FROM
          drafts.drafters d
        JOIN drafts.people p on d.person_id = p.id
        WHERE d.id = ANY(@assignedIds)
        UNION ALL
        SELECT
          dt.id AS {nameof(ParticipantNameRow.ParticipantId)},
          dt.name AS {nameof(ParticipantNameRow.ParticipantName)}
        FROM drafts.drafter_teams dt
        WHERE
          dt.id = ANY(@assignedIds)
        """;

      nameById = (await connection.QueryAsync<ParticipantNameRow>(
        new CommandDefinition(
          commandText: nameSql,
          parameters: new { assignedIds },
          cancellationToken: cancellationToken)))
        .ToDictionary(r => r.ParticipantId, r => r.ParticipantName);
    }

    var positions = positionRows
      .Select(r =>
      {
        DraftPositionAssignmentResponse? assignment = null;

        if (r.AssignedTo_ParticipantId.HasValue && r.AssignedTo_ParticipantKind.HasValue)
        {
          nameById.TryGetValue(r.AssignedTo_ParticipantId.Value, out var name);
          assignment = new DraftPositionAssignmentResponse
          {
            ParticipantId = r.AssignedTo_ParticipantId.Value,
            ParticipantKind = ParticipantKind.FromValue(r.AssignedTo_ParticipantKind.Value),
            ParticipantName = name
          };
        }

        var picks = string.IsNullOrEmpty(r.Picks)
          ? Array.Empty<int>()
          : r.Picks.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

        return new DraftPositionResponse
        {
          PublicId = r.PublicId,
          Name = r.Name,
          Picks = picks,
          HasBonusVeto = r.HasBonusVeto,
          HasBonusVetoOverride = r.HasBonusVetoOverride,
          AssignedTo = assignment
        };
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new ListDraftPositionsResponse
    {
      Positions = positions
    });
  }

  private sealed record PositionRow(
    string PublicId,
    string Name,
    string Picks,
    bool HasBonusVeto,
    bool HasBonusVetoOverride,
    Guid? AssignedTo_ParticipantId,
    int? AssignedTo_ParticipantKind);

  private sealed record ParticipantNameRow(
    Guid ParticipantId,
    string ParticipantName);
}
