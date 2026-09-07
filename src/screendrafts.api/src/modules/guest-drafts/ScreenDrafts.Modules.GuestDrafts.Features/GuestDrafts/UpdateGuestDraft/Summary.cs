using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.UpdateGuestDraft;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Update a guest draft";
    Description =
      "Owner-only. Title and DraftDate are always editable. Type can be changed too, but only before the draft starts -- changing it rebuilds the board from scratch (any bonus-token awards from the old board's assignments are revoked), so pass NumberOfPicks/Positions when switching to a non-fixed type (MiniMega/Super/Mega).";
    Response(StatusCodes.Status204NoContent, "Guest draft updated.");
    Response(
      StatusCodes.Status400BadRequest,
      "Invalid UpdateGuestDraftRequest, draft already started, or positions don't match NumberOfPicks."
    );
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can update it.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
  }
}
