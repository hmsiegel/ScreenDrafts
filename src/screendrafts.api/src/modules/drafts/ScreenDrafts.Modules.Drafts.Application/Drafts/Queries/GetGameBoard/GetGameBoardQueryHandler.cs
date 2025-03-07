namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetGameBoard;

internal sealed class GetGameBoardQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetGameBoardQuery, GameBoardResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GameBoardResponse>> Handle(GetGameBoardQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $@"
        SELECT
          gb.id AS {nameof(GameBoardResponse.Id)},
          gb.draft_id AS {nameof(GameBoardResponse.DraftId)},
          json_agg(
            json_build_object(
              'name', dp.name,
              'picks', dp.picks,
              'hasBonusVeto', dp.has_bonus_veto,
              'hasBonusVetoOverride', dp.has_bonus_veto_override,
              'id', dp.id,
              'drafter_id', dp.drafter_id
            )
          ) AS {nameof(GameBoardResponse.DraftPositions)}
        FROM drafts.game_boards gb
        JOIN drafts.draft_positions dp ON gb.id = dp.game_board_id
        WHERE gb.draft_id = @DraftId
        GROUP BY gb.id, gb.draft_id
        ";

    var gameBoard = await connection.QuerySingleOrDefaultAsync<GameBoardResponse>(
      sql,
      request);

    return gameBoard is not null
      ? Result.Success(gameBoard)
      : Result.Failure<GameBoardResponse>(DraftErrors.NotFound(request.DraftId));
  }
}
