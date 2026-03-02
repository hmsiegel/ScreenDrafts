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
    Policies(DraftsAuth.Permissions.DraftsList);
  }

  public override async Task HandleAsync(ListDraftsRequest req, CancellationToken ct)
  {
    var includePatreonOnly = User.HasClaim(
      c => c.Type == "permissions" && c.Value == DraftsAuth.Permissions.PatreonSearch);

    var query = new ListDraftsQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      FromDate = req.FromDate,
      ToDate = req.ToDate,
      DraftType = req.DraftType,
      CategoryPublicId = req.CategoryPublicId,
      MinDrafters = req.MinDrafters,
      MaxDrafters = req.MaxDrafters,
      MinPicks = req.MinPicks,
      MaxPicks = req.MaxPicks,
      Q = req.Q,
      SortBy = req.SortBy,
      Dir = req.Dir,
      IncludePatreonOnly = includePatreonOnly
    };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
