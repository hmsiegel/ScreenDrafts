using ScreenDrafts.Modules.GuestDrafts.Features.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyCommissionerOverride;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyCommissionerOverride;

internal sealed class Endpoint : ScreenDraftsEndpoint<ApplyGuestDraftCommissionerOverrideRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.PickCommissionerOverride);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_ApplyCommissionerOverride)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftApplyCommissionerOverride);
  }

  public override async Task HandleAsync(
    ApplyGuestDraftCommissionerOverrideRequest req,
    CancellationToken ct
  )
  {
    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new ApplyCommissionerOverrideCommand
    {
      GuestDraftPublicId = req.PublicId,
      PlayOrder = req.PlayOrder,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
