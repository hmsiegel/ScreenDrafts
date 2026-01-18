
namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, DrafterCollectionResponse>
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
    Policies(Features.Permissions.DrafterList);
  }
  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query(req);
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
