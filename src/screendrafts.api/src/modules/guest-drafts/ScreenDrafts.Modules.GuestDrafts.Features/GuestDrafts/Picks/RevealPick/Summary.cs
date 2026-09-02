using FastEndpoints;
using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.RevealPick;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.RevealPick;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Reveal a pick";
    Description =
      "Only the pick's designated revealer (assigned at play time -- deterministic for 2 participants, random draw otherwise) may reveal it.";
    Response(StatusCodes.Status204NoContent, "Pick revealed.");
    Response(StatusCodes.Status400BadRequest, "Already revealed.");
    Response(StatusCodes.Status403Forbidden, "You are not this pick's designated revealer.");
    Response(
      StatusCodes.Status404NotFound,
      "Guest draft or pick not found, or caller is not a participant."
    );
  }
}
