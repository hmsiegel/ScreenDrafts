namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetStatsQueryOptions;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<StatsQueryOptionsResponse>
{
  public override void Configure()
  {
    Get(DraftReportingRoutes.StatsQueryOptions);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Stats)
        .WithName(ReportingOpenApi.Names.Stats_GetQueryOptions)
        .Produces<StatsQueryOptionsResponse>(StatusCodes.Status200OK);
    });

    // Signed-in users only, like POST /stats/query.
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var isPatreonMember = User.HasPermission(ReportingAuth.Permissions.StatsReadPatreon);

    var result = await Sender.Send(
      new GetStatsQueryOptionsQuery { IncludeAll = isPatreonMember },
      ct
    );

    await this.SendOkAsync(result, ct);
  }
}
