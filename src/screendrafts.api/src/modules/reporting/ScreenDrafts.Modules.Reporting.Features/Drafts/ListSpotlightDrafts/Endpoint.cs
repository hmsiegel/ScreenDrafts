namespace ScreenDrafts.Modules.Reporting.Features.Drafts.ListSpotlightDrafts;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<ListSpotlightDraftsRequest, ListSpotlightDraftsResponse>
{
  public override void Configure()
  {
    Get(DraftReportingRoutes.Spotlights);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Spotlight)
        .WithName(ReportingOpenApi.Names.Spotlight_GetSpotlights)
        .Produces<PagedResult<ListSpotlightDraftsResponse>>(StatusCodes.Status200OK);
    });
    Policies(ReportingAuth.Permissions.SpotlightManage);
  }

  public override async Task HandleAsync(ListSpotlightDraftsRequest req, CancellationToken ct)
  {
    var query = new ListSpotlightDraftsQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      ExcludeActive = req.ExcludeActive,
      Query = req.Query,
      DraftType = req.DraftType,
    };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
