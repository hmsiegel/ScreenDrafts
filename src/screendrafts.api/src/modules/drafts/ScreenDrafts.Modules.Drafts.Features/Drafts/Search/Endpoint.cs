namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Search;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchDraftsRequest, SearchDraftsResponse>
{
  public override void Configure()
  {
    Get(DraftRoutes.Search);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_SearchDrafts)
      .Produces<PagedResult<SearchDraftsResponse>>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(DraftsAuth.Permissions.DraftsList);
  }

  public override async Task HandleAsync(SearchDraftsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var query = new SearchDraftsQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      Name = req.Name,
      CampaignPublicId = req.CampaignPublicId,
      CategoryPublicId = req.CategoryPublicId,
      DraftType = req.DraftType
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
