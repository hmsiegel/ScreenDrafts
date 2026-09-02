namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetCustomPositionsRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.CustomBoardLayout);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_SetCustomPositions)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftSetBoard);
  }

  public override async Task HandleAsync(SetCustomPositionsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new SetCustomPositionsCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      Positions = req.Positions,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
