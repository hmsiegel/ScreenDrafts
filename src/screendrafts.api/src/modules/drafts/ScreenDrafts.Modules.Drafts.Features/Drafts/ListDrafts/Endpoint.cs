namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<ListDraftsRequest, PagedResult<ListDraftsResponse>>
{
  public override void Configure()
  {
    Get(DraftRoutes.Base);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_ListDrafts)
        .Produces<PagedResult<ListDraftsResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(ListDraftsRequest req, CancellationToken ct)
  {
    var includePatreon =
      User.Identity?.IsAuthenticated == true
      && User.HasPermission(DraftsAuth.Permissions.DraftReadPatreon);

    var query = new ListDraftsQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      FromDate = req.FromDate,
      ToDate = req.ToDate,
      DraftType = req.DraftType,
      CategoryPublicId = req.CategoryPublicId,
      CampaignPublicId = req.CampaignPublicId,
      MinDrafters = req.MinDrafters,
      MaxDrafters = req.MaxDrafters,
      MinPicks = req.MinPicks,
      MaxPicks = req.MaxPicks,
      Q = req.Q,
      SortBy = req.SortBy,
      Dir = req.Dir,
      IncludePatreon = includePatreon,
    };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
