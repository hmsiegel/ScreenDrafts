namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetSiteStats;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetSiteStatsResponse>
{
  public override void Configure()
  {
    Get(ReportingRoutes.Stats);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Stats)
        .WithName(ReportingOpenApi.Names.Stats_GetSiteStats)
        .Produces<GetSiteStatsResponse>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var isPatreonMember = User.HasPermission(ReportingAuth.Permissions.StatsReadPatreon);

    var query = new GetSiteStatsQuery { IsPatreonMember = isPatreonMember };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
