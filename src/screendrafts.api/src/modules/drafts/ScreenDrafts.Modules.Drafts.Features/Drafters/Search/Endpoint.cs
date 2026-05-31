namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Search;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchDraftersRequest, PagedResult<SearchDraftersResponse>>
{
  public override void Configure()
  {
    Get(DrafterRoutes.Search);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafters)
        .WithName(DraftsOpenApi.Names.Drafters_SearchDrafters)
        .Produces<PagedResult<SearchDraftersResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.DrafterRead);
  }

  public override async Task HandleAsync(SearchDraftersRequest req, CancellationToken ct)
  {
    var query = new SearchDraftersQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      Name = req.Name,
      IsRetired = req.IsRetired,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
