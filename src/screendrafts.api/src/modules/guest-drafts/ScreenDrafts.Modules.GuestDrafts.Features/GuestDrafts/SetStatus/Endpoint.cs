namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SetGuestDraftStatusRequest, SetGuestDraftStatusResponse>
{
  public override void Configure()
  {
    Put(GuestDraftsRoutes.GuestDraftStatus);
    Description(x => x
      .WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
      .WithName(GuestDraftsOpenApi.Names.GuestDrafts_SetStatus)
      .Produces<SetGuestDraftStatusResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound));
    Policies(GuestDraftsAuth.Permissions.GuestDraftSetStatus);
  }

  public override async Task HandleAsync(SetGuestDraftStatusRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new SetGuestDraftStatusCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      Action = req.Action
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
