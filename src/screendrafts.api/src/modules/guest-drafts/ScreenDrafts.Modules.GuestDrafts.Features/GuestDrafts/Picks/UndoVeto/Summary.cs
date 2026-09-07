using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoVeto;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Undo a veto on a pick";
    Description =
      "Owner-only, break-glass action. Restores the pick to the board and refunds the veto token to the original issuer. Fails if the veto has already been overridden.";
    Response(StatusCodes.Status204NoContent, "Veto undone.");
    Response(
      StatusCodes.Status400BadRequest,
      "Pick isn't vetoed, or the veto has already been overridden."
    );
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can undo vetoes.");
    Response(StatusCodes.Status404NotFound, "Guest draft or pick not found.");
  }
}
