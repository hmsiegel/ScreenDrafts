namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Search;

internal sealed class Endpoint : ScreenDraftsEndpoint<SearchDrafterTeamsRequest, SearchDrafterTeamsResponse>
{
  public override void Configure()
  {
    Get(DrafterTeamRoutes.Search);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DrafterTeams)
      .WithName(DraftsOpenApi.Names.DrafterTeams_SearchDrafterTeams)
      .Produces<PagedResult<SearchDrafterTeamsResponse>>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(DraftsAuth.Permissions.DrafterTeamRead);
  }

  public override async Task HandleAsync(SearchDrafterTeamsRequest req, CancellationToken ct)
  {
    var query = new SearchDrafterTeamsQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      Name = req.Name
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
