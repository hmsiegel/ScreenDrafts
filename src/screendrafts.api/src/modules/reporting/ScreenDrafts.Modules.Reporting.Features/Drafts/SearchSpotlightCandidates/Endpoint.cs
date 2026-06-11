namespace ScreenDrafts.Modules.Reporting.Features.Drafts.SearchSpotlightCandidates;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchSpotlightCandidatesRequest, SearchSpotlightCandidatesResponse>
{
  public override void Configure()
  {
    Get(DraftReportingRoutes.Candidates);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Spotlight)
        .WithName(ReportingOpenApi.Names.Spotlight_SearchCandidates)
        .Produces<SearchSpotlightCandidatesResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(ReportingAuth.Permissions.SpotlightManage);
  }

  public override async Task HandleAsync(SearchSpotlightCandidatesRequest req, CancellationToken ct)
  {
    var query = new SearchSpotlightCandidatesQuery
    {
      Query = req.Query,
      Page = req.Page,
      PageSize = req.PageSize,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
