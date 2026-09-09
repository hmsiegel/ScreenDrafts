using ScreenDrafts.Modules.GuestDrafts.Features.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.UndoVeto;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.UndoVeto;

internal sealed class Endpoint : ScreenDraftsEndpoint<UndoGuestDraftVetoRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.PickUndoVeto);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_UndoVeto)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftUndoVeto);
  }

  public override async Task HandleAsync(UndoGuestDraftVetoRequest req, CancellationToken ct)
  {
    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new UndoVetoCommand
    {
      GuestDraftPublicId = req.PublicId,
      PlayOrder = req.PlayOrder,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
