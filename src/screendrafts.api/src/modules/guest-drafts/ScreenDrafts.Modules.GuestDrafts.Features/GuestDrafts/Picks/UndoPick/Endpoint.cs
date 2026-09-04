namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<UndoGuestDraftPickRequest>
{
  public override void Configure()
  {
    Delete(GuestDraftsRoutes.PickByPlayOrder);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_UndoPick)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftUndoPick);
  }

  public override async Task HandleAsync(UndoGuestDraftPickRequest req, CancellationToken ct)
  {
    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new UndoPickCommand
    {
      GuestDraftPublicId = req.PublicId,
      PlayOrder = req.PlayOrder,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
