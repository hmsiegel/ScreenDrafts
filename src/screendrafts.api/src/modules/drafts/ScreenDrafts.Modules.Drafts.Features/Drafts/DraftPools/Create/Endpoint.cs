namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateDraftPoolRequest>
{
  public override void Configure()
  {
    Post(DraftRoutes.Pool);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftPools)
      .WithName(DraftsOpenApi.Names.DraftPools_CreatePool)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPoolCreate);
  }

  public override async Task HandleAsync(CreateDraftPoolRequest req, CancellationToken ct)
  {
    var command = new CreateDraftPoolCommand
    {
      PublicId = req.PublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
