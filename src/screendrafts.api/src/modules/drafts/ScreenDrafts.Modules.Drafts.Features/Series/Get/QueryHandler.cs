namespace ScreenDrafts.Modules.Drafts.Features.Series.Get;

internal sealed partial class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, SeriesResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<SeriesResponse>> Handle(Query request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        SELECT
          s.publicId AS {nameof(SeriesResponse.PublicId)},
          s.name AS {nameof(SeriesResponse.Name)},
          s.kind AS {nameof(SeriesResponse.Kind)},
          s.canonical_policy AS {nameof(SeriesResponse.CanonicalPolicy)},
          s.continuity_scope AS {nameof(SeriesResponse.ContinuityScope)},
          s.continuity_date_rule AS {nameof(SeriesResponse.ContinuityDateRule)},
          s.allowed_draft_types AS {nameof(SeriesResponse.AllowedDraftTypesMask)},
          s.default_draft_type AS {nameof(SeriesResponse.DefaultDraftType)}
        FROM
          drafts.series s
        WHERE
          s.publicId = @PublicId
      """;

    var row = await connection.QuerySingleOrDefaultAsync<SeriesRow>(new CommandDefinition(
      sql,
      new { request.PublicId },
      cancellationToken: cancellationToken));

    return row is not null
      ? Result.Success(QueryMapping.Map(row))
      : Result.Failure<SeriesResponse>(CampaignErrors.NotFound(request.PublicId));
  }
}


