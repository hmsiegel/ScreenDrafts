namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Search;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchHostRequest, PagedResult<SearchHostResponse>>
{
  public override void Configure()
  {
    Get(HostRoutes.Search);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Hosts)
      .WithName(DraftsOpenApi.Names.Hosts_SearchHosts)
      .Produces<PagedResult<SearchHostResponse>>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.HostRead);
  }

  public override async Task HandleAsync(SearchHostRequest req, CancellationToken ct)
  {
    var query = new SearchHostQuery
    {
      Name = req.Name,
      Page = req.Page ?? 1,
      PageSize = req.PageSize ?? 10,
      SortBy = req.SortBy,
      HasBeenPrimary = req.HasBeenPrimary
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
