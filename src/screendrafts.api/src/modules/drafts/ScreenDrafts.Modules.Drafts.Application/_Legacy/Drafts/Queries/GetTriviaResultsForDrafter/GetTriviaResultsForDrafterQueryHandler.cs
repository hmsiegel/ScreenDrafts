namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetTriviaResultsForDrafter;

internal sealed class GetTriviaResultsForDrafterQueryHandler(IDbConnectionFactory dbConnectionFactory) 
  : IQueryHandler<GetTriviaResultsForDrafterQuery, TriviaResultDto>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<TriviaResultDto>> Handle(GetTriviaResultsForDrafterQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var query = $"""
        SELECT
          tr.id AS {nameof(TriviaResultDto.Id)},
          tr.position {nameof(TriviaResultDto.Position)},
          tr.questions_won AS {nameof(TriviaResultDto.QuestionsWon)},
          tr.draft_id AS {nameof(TriviaResultDto.DraftId)},
          tr.drafter_id AS {nameof(TriviaResultDto.DrafterId)}
        FROM
          drafts.trivia_results tr
        WHERE
          tr.draft_id = @DraftId
          AND tr.drafter_id = @DrafterId;
        """;

    var result = await connection.QueryFirstOrDefaultAsync<TriviaResultDto>(query, request);

    return result is not null
      ? Result.Success(result)
      : Result.Failure<TriviaResultDto>(DraftErrors.NotFound(request.DraftId));
  }
}
