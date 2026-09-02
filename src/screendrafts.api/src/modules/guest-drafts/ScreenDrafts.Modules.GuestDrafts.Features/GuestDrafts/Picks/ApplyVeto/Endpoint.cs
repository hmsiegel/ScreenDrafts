namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVeto;

internal sealed class Endpoint : ScreenDraftsEndpoint<ApplyVetoRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.PickVeto);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_ApplyVeto)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftApplyVeto);
  }

  public override async Task HandleAsync(ApplyVetoRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new ApplyVetoCommand
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
