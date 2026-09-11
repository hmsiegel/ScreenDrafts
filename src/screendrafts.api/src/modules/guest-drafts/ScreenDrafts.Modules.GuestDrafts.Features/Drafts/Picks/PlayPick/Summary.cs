using FastEndpoints;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.PlayPick;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.PlayPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Play a pick in a guest draft";
    Description =
      "The caller plays as themselves -- validates the movie exists via the Movies module, then plays the pick at the given board position and play order.";
    Response(StatusCodes.Status204NoContent, "Pick played.");
    Response(
      StatusCodes.Status400BadRequest,
      "Movie already picked, position already landed, or the draft isn't in progress."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "You do not have permission to play a pick in this guest draft."
    );
    Response(
      StatusCodes.Status404NotFound,
      "Guest draft not found, or caller is not a participant."
    );
  }
}
