namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.RevealPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<RevealGuestDraftPickRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.PickReveal);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_RevealPick)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftRevealPick);
  }

  public override async Task HandleAsync(RevealGuestDraftPickRequest req, CancellationToken ct)
  {
    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new RevealPickCommand
    {
      GuestDraftPublicId = req.PublicId,
      PlayOrder = req.PlayOrder,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
