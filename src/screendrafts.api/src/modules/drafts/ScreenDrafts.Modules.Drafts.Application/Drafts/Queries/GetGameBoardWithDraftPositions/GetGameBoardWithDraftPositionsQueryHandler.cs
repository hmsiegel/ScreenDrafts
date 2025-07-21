namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetGameBoardWithDraftPositions;

internal sealed class GetGameBoardWithDraftPositionsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetGameBoardWithDraftPositionsQuery, GetGameBoardWithDraftPositionsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetGameBoardWithDraftPositionsResponse>> Handle(GetGameBoardWithDraftPositionsQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $@"
        SELECT
          gb.id AS {nameof(GetGameBoardWithDraftPositionsResponse.Id)},
          gb.draft_id AS {nameof(GetGameBoardWithDraftPositionsResponse.DraftId)},
          json_agg(
            json_build_object(
              'name', dp.name,
              'picks', dp.picks,
              'hasBonusVeto', dp.has_bonus_veto,
              'hasBonusVetoOverride', dp.has_bonus_veto_override,
              'id', dp.id,
              'drafter_id', dp.drafter_id
            )
          ) AS {nameof(GetGameBoardWithDraftPositionsResponse.DraftPositions)}
        FROM drafts.game_boards gb
        JOIN drafts.draft_positions dp ON gb.id = dp.game_board_id
        WHERE gb.draft_id = @DraftId
        GROUP BY gb.id, gb.draft_id
        ";

    var gameBoard = await connection.QuerySingleOrDefaultAsync<GetGameBoardWithDraftPositionsResponse>(
      sql,
      request);

    return gameBoard is not null
      ? Result.Success(gameBoard)
      : Result.Failure<GetGameBoardWithDraftPositionsResponse>(DraftErrors.NotFound(request.DraftId));
  }
}
