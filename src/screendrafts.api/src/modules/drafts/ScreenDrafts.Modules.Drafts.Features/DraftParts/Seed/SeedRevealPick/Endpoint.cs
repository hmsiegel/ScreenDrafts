namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedRevealPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<SeedRevealPickRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SeedRevealPick);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_SeedRevealPick)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftSeed);
  }

  public override async Task HandleAsync(SeedRevealPickRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new SeedRevealPickCommand
    {
      DraftPartId = req.DraftPartId,
      PlayOrder = req.PlayOrder,
      ActedByPublicId = req.ActedByPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
