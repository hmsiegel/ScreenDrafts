namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetTitleHonorificsRequest, GetTitleHonorificsResponse>
{
  public override void Configure()
  {
    Get(DraftReportingRoutes.StatsTitles);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Stats)
        .WithName(ReportingOpenApi.Names.Stats_GetTitleHonorifics)
        .Produces<GetTitleHonorificsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetTitleHonorificsRequest req, CancellationToken ct)
  {
    // The Patreon/Speed toggle only takes effect for Patreon members, as in the Record Book.
    var isPatreonMember = User.HasPermission(ReportingAuth.Permissions.StatsReadPatreon);

    var query = new GetTitleHonorificsQuery
    {
      Level = req.Level,
      Search = req.Search,
      Sort = req.Sort,
      Page = req.Page,
      PageSize = req.PageSize,
      IncludeAll = req.IncludeAll && isPatreonMember,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
