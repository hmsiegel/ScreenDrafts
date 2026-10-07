namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed class QueryStatsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<QueryStatsQuery, QueryStatsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<QueryStatsResponse>> Handle(
    QueryStatsQuery request,
    CancellationToken cancellationToken
  )
  {
    var validation = StatsQueryValidator.Validate(request);

    if (validation.Spec is null)
    {
      return Result.Failure<QueryStatsResponse>(validation.Error!);
    }

    var spec = validation.Spec;
    var metric = StatsMetrics.Find(spec.Metric)!;

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var dataset = await QueryDatasetLoader.LoadAsync(
      connection,
      request.IncludeAll,
      cancellationToken
    );

    var result = StatsQueryEngine.Run(dataset, spec);

    return Result.Success(
      new QueryStatsResponse
      {
        Metric = metric.Code,
        MetricLabel = metric.Label,
        GroupBy = spec.GroupBy,
        Format = metric.Format,
        IncludesNonCanonical = request.IncludeAll,
        TotalGroups = result.TotalGroups,
        Truncated = result.TotalGroups > result.Rows.Count,
        Rows = result.Rows,
      }
    );
  }
}
