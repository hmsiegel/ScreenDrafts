﻿namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;

internal sealed class GetDraftPicksByDraftQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetDraftPicksByDraftQuery, List<DraftPickResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<List<DraftPickResponse>>> Handle(GetDraftPicksByDraftQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql =
      $"""
        SELECT
          p.position AS {nameof(DraftPickResponse.Position)},
          p.movie_id AS {nameof(DraftPickResponse.MovieId)},
          m.movie_title AS {nameof(DraftPickResponse.MovieTitle)},
          p.drafter_id AS {nameof(DraftPickResponse.DrafterId)},
          d.name AS {nameof(DraftPickResponse.DrafterName)}
          FROM drafts.picks p
          INNER JOIN drafts.movies m ON p.movie_id = m.id
          INNER JOIN drafts.drafters d ON p.drafter_id = d.id
          WHERE p.draft_id = @DraftId
          ORDER BY p.position DESC
      """;

    List<DraftPickResponse> draftPicks = [.. await connection.QueryAsync<DraftPickResponse>(sql, request)];

    if (draftPicks.Count == 0)
    {
      return Result.Failure<List<DraftPickResponse>>(DraftErrors.PicksNotFound);
    }

    return draftPicks;
  }
}
