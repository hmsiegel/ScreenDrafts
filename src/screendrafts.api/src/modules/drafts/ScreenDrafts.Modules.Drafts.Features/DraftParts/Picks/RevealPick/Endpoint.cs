namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<RevealPickRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.RevealPick);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_PickReveal)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PickReveal);
  }

  public override async Task HandleAsync(RevealPickRequest req, CancellationToken ct)
  {
    var actorPublicId = User.GetPublicId();

    if (string.IsNullOrWhiteSpace(actorPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var command = new RevealPickCommand
    {
      DraftPartId = req.DraftPartId,
      PlayOrder = req.PlayOrder,
      ActorPublicId = actorPublicId
    }; 

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
