namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListDraftersRequest, DrafterCollectionResponse>
{
  public override void Configure()
  {
    Get(DrafterRoutes.Drafters);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafters)
       .WithName(DraftsOpenApi.Names.Drafters_ListDrafters)
       .Produces<DrafterCollectionResponse>(StatusCodes.Status200OK)
       .Produces(StatusCodes.Status400BadRequest)
       .Produces(StatusCodes.Status401Unauthorized)
       .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.DrafterList);
  }
  public override async Task HandleAsync(ListDraftersRequest req, CancellationToken ct)
  {
    var query = new ListDraftersQuery(req);
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}


