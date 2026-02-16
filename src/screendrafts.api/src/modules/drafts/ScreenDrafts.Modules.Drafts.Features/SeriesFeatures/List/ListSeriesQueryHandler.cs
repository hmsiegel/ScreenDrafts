namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.List;

internal sealed class ListSeriesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListSeriesQuery, SeriesCollectionResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<SeriesCollectionResponse>> Handle(
    ListSeriesQuery request,
    CancellationToken cancellationToken)
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

    var rows = await connection.QueryAsync<SeriesRow>(new CommandDefinition(
      sql,
      cancellationToken: cancellationToken));

    var series = rows.Select(QueryMapping.Map).ToList();

    return Result.Success(new SeriesCollectionResponse(series));
  }
}



