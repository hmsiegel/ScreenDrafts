namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.List;

internal sealed class ListSeriesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListSeriesQuery, SeriesCollectionResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<SeriesCollectionResponse>> Handle(
    ListSeriesQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
        SELECT
          s.public_id AS {nameof(SeriesRow.PublicId)},
          s.description AS {nameof(SeriesRow.Description)},
          s.name AS {nameof(SeriesRow.Name)},
          s.kind AS {nameof(SeriesRow.Kind)},
          s.canonical_policy AS {nameof(SeriesRow.CanonicalPolicy)},
          s.continuity_scope AS {nameof(SeriesRow.ContinuityScope)},
          s.continuity_date_rule AS {nameof(SeriesRow.ContinuityDateRule)},
          s.allowed_draft_types AS {nameof(SeriesRow.AllowedDraftTypesMask)},
          s.default_draft_type AS {nameof(SeriesRow.DefaultDraftType)},
          s.is_deleted AS {nameof(SeriesRow.IsDeleted)}
        FROM
          drafts.series s
        WHERE
          (s.is_deleted = FALSE OR @IncludeDeleted = TRUE)
        ORDER BY
          s.name ASC
      """;

    var parameters = new DynamicParameters();
    parameters.Add("IncludeDeleted", request.IncludeDeleted, DbType.Boolean);

    var rows = await connection.QueryAsync<SeriesRow>(
      new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)
    );

    var series = rows.Select(QueryMapping.Map).ToList();

    return Result.Success(new SeriesCollectionResponse(series));
  }
}
