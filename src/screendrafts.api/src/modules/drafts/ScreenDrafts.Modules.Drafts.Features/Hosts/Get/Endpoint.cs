namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetHostRequest, GetHostResponse>
{
  public override void Configure()
  {
    Get(HostRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Hosts)
      .WithName(DraftsOpenApi.Names.Hosts_GetHostById)
      .Produces<GetHostResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.HostRead);
  }

  public override async Task HandleAsync(GetHostRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var query = new GetHostQuery 
    {
      HostPublicId = req.PublicId
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

