namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoPick;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
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

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userPublicId = User.GetUserPublicId();
    var publicId = Route<string>("publicId");
    var playOrder = Route<int>("playOrder");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new UndoPickCommand
    {
      GuestDraftPublicId = publicId,
      PlayOrder = playOrder,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
