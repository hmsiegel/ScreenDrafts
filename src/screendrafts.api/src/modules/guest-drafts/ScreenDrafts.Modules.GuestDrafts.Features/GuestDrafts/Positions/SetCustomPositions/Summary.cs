using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set a custom board layout for a guest draft";
    Description =
      "Owner-only. Owner-supplied board layout for MiniMega/Super/Mega guest drafts -- name, pick slots, and bonus flags per position. Fails for Standard/MiniSuper, which use SetFixedBoardLayout instead.";
    Response(StatusCodes.Status204NoContent, "Board layout applied.");
    Response(
      StatusCodes.Status400BadRequest,
      "Invalid position layout (duplicate slots, wrong position count, etc.), or this draft type has a fixed layout."
    );
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can set up its board.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
  }
}
