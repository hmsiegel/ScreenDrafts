namespace ScreenDrafts.Modules.Drafts.Features.Predictions.GetDraftPartPredictors;

internal sealed class GetDraftPartPredictorsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetDraftPartPredictorsQuery, GetDraftPartPredictorsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetDraftPartPredictorsResponse>> Handle(
    GetDraftPartPredictorsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      SELECT
        pc.public_id                                   AS {nameof(Row.ContestantPublicId)},
        pc.display_name                                AS {nameof(Row.ContestantDisplayName)},
        subp.public_id                                 AS {nameof(
        Row.AllowedSubmitterPersonPublicId
      )},
        (subp.first_name || ' ' || subp.last_name)      AS {nameof(Row.AllowedSubmitterDisplayName)}
      FROM drafts.draft_part_predictors dpp
      JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
      JOIN drafts.prediction_contestants pc ON pc.id = dpp.contestant_id
      LEFT JOIN drafts.people subp ON subp.id = dpp.allowed_submitter_person_id
      WHERE dp.public_id = @DraftPartPublicId
      ORDER BY pc.display_name
      """;

    var rows = (
      await connection.QueryAsync<Row>(
        new CommandDefinition(
          sql,
          new { request.DraftPartPublicId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    return Result.Success(
      new GetDraftPartPredictorsResponse
      {
        Predictors =
        [
          .. rows.Select(r => new PredictorResponse
          {
            ContestantPublicId = r.ContestantPublicId,
            ContestantDisplayName = r.ContestantDisplayName,
            AllowedSubmitterPersonPublicId = r.AllowedSubmitterPersonPublicId,
            AllowedSubmitterDisplayName = r.AllowedSubmitterDisplayName,
          }),
        ],
      }
    );
  }

  private sealed record Row(
    string ContestantPublicId,
    string ContestantDisplayName,
    string? AllowedSubmitterPersonPublicId,
    string? AllowedSubmitterDisplayName
  );
}
