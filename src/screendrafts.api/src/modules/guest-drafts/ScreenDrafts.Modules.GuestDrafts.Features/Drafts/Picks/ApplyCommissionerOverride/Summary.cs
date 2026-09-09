using FastEndpoints;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyCommissionerOverride;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyCommissionerOverride;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Apply a commissioner override to a pick";
    Description =
      "Owner-only, break-glass action. Permanently removes the pick from the board -- unlike a veto, a commissioner-overridden pick is not eligible for re-pick.";
    Response(StatusCodes.Status204NoContent, "Commissioner override applied.");
    Response(
      StatusCodes.Status400BadRequest,
      "A commissioner override has already been applied to this pick."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "Only the guest draft's owner can apply commissioner overrides."
    );
    Response(StatusCodes.Status404NotFound, "Guest draft or pick not found.");
  }
}
