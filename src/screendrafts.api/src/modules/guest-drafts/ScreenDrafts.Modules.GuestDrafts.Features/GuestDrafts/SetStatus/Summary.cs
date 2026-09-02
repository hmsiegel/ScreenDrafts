using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetStatus;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Start or complete a guest draft";
    Description = "Owner-only. Start needs at least 2 participants and a fully assigned board; Complete needs every board position to have a landed pick.";
    Response<SetGuestDraftStatusResponse>(StatusCodes.Status200OK, "The draft's new status.");
    Response(StatusCodes.Status400BadRequest, "The requested transition isn't valid for the draft's current state.");
    Response(StatusCodes.Status403Forbidden, "Only the guest draft's owner can change its status.");
    Response(StatusCodes.Status404NotFound, "Guest draft not found.");
  }
}
