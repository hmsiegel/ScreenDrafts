namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPositionsByGameBoard;

internal sealed class GetDraftPositionsByGameBoardQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetDraftPositionsByGameBoardQuery, IReadOnlyCollection<DraftPositionResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyCollection<DraftPositionResponse>>> Handle(
    GetDraftPositionsByGameBoardQuery request,
    CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
        SELECT
          id AS {nameof(DraftPositionResponse.Id)},
          name AS {nameof(DraftPositionResponse.Name)},
          picks AS {nameof(DraftPositionResponse.Picks)},
          has_bonus_veto AS {nameof(DraftPositionResponse.HasBonusVeto)},
          has_bonus_veto_override AS {nameof(DraftPositionResponse.HasBonusVetoOveride)},
          drafter_id AS {nameof(DraftPositionResponse.DrafterId)},
          drafter_team_id AS {nameof(DraftPositionResponse.DrafterTeamId)}
        FROM drafts.draft_positions
        WHERE game_board_id = @GameBoardId
        """;

    List<DraftPositionResponse> draftPositions = [.. await connection.QueryAsync<DraftPositionResponse>(sql, request)];

    return draftPositions;
  }
}
