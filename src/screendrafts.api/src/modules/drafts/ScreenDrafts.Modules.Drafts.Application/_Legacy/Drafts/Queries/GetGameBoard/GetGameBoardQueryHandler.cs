namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetGameBoard;

internal sealed class GetGameBoardQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetGameBoardQuery, GameBoardResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GameBoardResponse>> Handle(GetGameBoardQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $@"
        SELECT
          gb.id AS {nameof(GameBoardResponse.Id)},
          gb.draft_id AS {nameof(GameBoardResponse.DraftId)}
        FROM drafts.game_boards gb
        WHERE gb.draft_id = @DraftId
        ";

    var gameBoard = await connection.QuerySingleOrDefaultAsync<GameBoardResponse>(sql, request);

    return gameBoard is not null
      ? Result.Success(gameBoard)
      : Result.Failure<GameBoardResponse>(DraftErrors.NotFound(request.DraftId));
  }
}
