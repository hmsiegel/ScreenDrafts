using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetFixedBoardLayout;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Apply the fixed board layout to a guest draft";
    Description =
      "Owner-only. Applies the fixed Standard or MiniSuper board layout, per the draft's GuestDraftType. Fails for any other draft type.";
    Response(StatusCodes.Status204NoContent, "Board layout applied.");
    Response(
      StatusCodes.Status400BadRequest,
      "Draft type does not have a fixed layout, or the board has already been set up."
    );
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can set up its board.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
  }
}
