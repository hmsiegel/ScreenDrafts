namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVetoOverrides;

internal sealed class Endpoint : ScreenDraftsEndpoint<ApplyGuestDraftVetoOverrideRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.PickVetoOverride);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_ApplyVetoOverride)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftApplyVetoOverride);
  }

  public override async Task HandleAsync(
    ApplyGuestDraftVetoOverrideRequest req,
    CancellationToken ct
  )
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new ApplyVetoOverrideCommand
    {
      GuestDraftPublicId = req.PublicId,
      PlayOrder = req.PlayOrder,
      CallerUserPublicId = userPublicId,
      Note = req.Note,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
