namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

internal sealed class Endpoint : ScreenDraftsEndpoint<QueryStatsRequest, QueryStatsResponse>
{
  public override void Configure()
  {
    Post(DraftReportingRoutes.StatsQuery);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Stats)
        .WithName(ReportingOpenApi.Names.Stats_Query)
        .Produces<QueryStatsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    });

    // Any signed-in user may run a query. No permission is required, so anonymous calls are not allowed.
  }

  public override async Task HandleAsync(QueryStatsRequest req, CancellationToken ct)
  {
    // The Patreon/Speed toggle only takes effect for Patreon members, as in GET /stats and the Record Book.
    var isPatreonMember = User.HasPermission(ReportingAuth.Permissions.StatsReadPatreon);

    var query = new QueryStatsQuery
    {
      Metric = req.Metric,
      GroupBy = req.GroupBy,
      Series = req.Series,
      DraftTypes = req.DraftTypes,
      EpisodeFrom = req.EpisodeFrom,
      EpisodeTo = req.EpisodeTo,
      MinAppearances = req.MinAppearances,
      Ascending = req.Ascending,
      Limit = req.Limit,
      IncludeAll = req.IncludeAll && isPatreonMember,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
