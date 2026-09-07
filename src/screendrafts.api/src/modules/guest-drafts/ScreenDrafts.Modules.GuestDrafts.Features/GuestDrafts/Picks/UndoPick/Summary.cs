using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.UndoPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Undo a pick";
    Description =
      "Owner-only, break-glass action. Removes the pick entirely -- no-op (still succeeds) if no pick exists at that play order, mirroring canonical DraftPart.UndoPick.";
    Response(StatusCodes.Status204NoContent, "Pick removed (or already absent).");
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can undo picks.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
  }
}
