using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraftPositionsByGameBoard;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraftPosition;

internal sealed class GetDraftPositionQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetDraftPositionQuery, DraftPositionResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  public async Task<Result<DraftPositionResponse>> Handle(
      GetDraftPositionQuery request,
      CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);
    const string sql =
        $"""
          SELECT
              id AS {nameof(DraftPositionResponse.Id)},
              name AS {nameof(DraftPositionResponse.Name)},
              picks AS {nameof(DraftPositionResponse.Picks)},
              has_bonus_veto AS {nameof(DraftPositionResponse.HasBonusVeto)},
              has_bonus_veto_override AS {nameof(DraftPositionResponse.HasBonusVetoOveride)},
              drafter_id AS {nameof(DraftPositionResponse.DrafterId)}
          FROM drafts.draft_positions
          WHERE game_board_id = @GameBoardId
          AND id = @PositionId
         """;

    var draftPosition = await connection.QueryFirstOrDefaultAsync<DraftPositionResponse>(sql, request);

    return draftPosition is not null
        ? Result.Success(draftPosition)
        : Result.Failure<DraftPositionResponse>(DraftPositionErrors.NotFound(request.PositionId));
  }
}
