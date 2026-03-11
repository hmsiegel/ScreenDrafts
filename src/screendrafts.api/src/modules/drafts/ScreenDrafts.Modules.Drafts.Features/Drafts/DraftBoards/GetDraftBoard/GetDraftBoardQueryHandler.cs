namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

internal sealed class GetDraftBoardQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  ICacheService cacheService)
  : IQueryHandler<GetDraftBoardQuery, GetDraftBoardResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly ICacheService _cacheService = cacheService;

  private static readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(60);

  public async Task<Result<GetDraftBoardResponse>> Handle(GetDraftBoardQuery request, CancellationToken cancellationToken)
  {
    var cacheKey = DraftsCacheKeys.DraftBoard(request.DraftId, request.UserId);

    var cachedResult = await _cacheService.GetAsync<GetDraftBoardResponse>(cacheKey, cancellationToken);

    if (cachedResult is not null)
    {
      return Result.Success(cachedResult);
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string boardSql =
      $"""
      SELECT 
        b.public_id AS PublicId,
        d.public_id AS DraftId
      FROM drafts.draft_boards b
      JOIN drafts.drafts d ON b.draft_id = d.id
      LEFT JOIN drafts.drafters dr ON dr.id = b.participant_id
      LEFT JOIN drafts.drafter_team_drafter dtm ON dtm.drafter_id = dr.id
      LEFT JOIN drafts.drafter_teams dt ON dt.id = dtm.drafter_team_id
                                        OR dt.id = b.participant_id
      LEFT JOIN drafts.people p ON p.id = dr.person_id
      WHERE d.public_id = @DraftId AND p.user_id = @UserId
      LIMIT 1;
      """;

    const string itemsSql =
      $"""
      SELECT
        bi.tmdb_id AS {nameof(DraftBoardItemResponse.TmdbId)},
        bi.notes AS {nameof(DraftBoardItemResponse.Notes)},
        bi.priority AS {nameof(DraftBoardItemResponse.Priority)}
      FROM drafts.draft_board_items bi
      JOIN drafts.draft_boards b ON b.id = bi.draft_board_id
      JOIN drafts.drafts d ON d.id = b.draft_id
      LEFT JOIN drafts.drafters dr ON dr.id = b.participant_id
      LEFT JOIN drafts.drafter_team_drafter dtm ON dtm.drafter_id = dr.id
      LEFT JOIN drafts.drafter_teams dt ON dt.id = dtm.drafter_team_id
                                        OR dt.id = b.participant_id
      LEFT JOIN drafts.people p ON p.id = dr.person_id
      WHERE d.public_id = @DraftId AND p.user_id = @UserId
      ORDER BY bi.priority ASC NULLS LAST, bi.tmdb_id ASC;
      """;

    var board = await connection.QuerySingleOrDefaultAsync<GetDraftBoardResponse>(
      new CommandDefinition(
        boardSql,
        new { request.DraftId, request.UserId },
        cancellationToken: cancellationToken));

    if (board is null)
    {
      return Result.Failure<GetDraftBoardResponse>(DraftBoardErrors.NotFoundForParticipant(request.UserId));
    }

    var items = await connection.QueryAsync<DraftBoardItemResponse>(
      new CommandDefinition(
        itemsSql,
        new { request.DraftId, request.UserId },
        cancellationToken: cancellationToken));

    board = board with { Items = [.. items] };

    await _cacheService.SetAsync(cacheKey, board, _cacheExpiration, cancellationToken);

    return Result.Success(board);
  }
}
