using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVetoOverrides;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Override a veto";
    Description =
      "The caller overrides as themselves. Not allowed on Standard guest drafts, or for a participant overriding their own pick's veto.";
    Response(StatusCodes.Status204NoContent, "Veto override applied.");
    Response(
      StatusCodes.Status400BadRequest,
      "Not allowed for this draft type, no active veto on this pick, overriding own pick, or no remaining overrides."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "You do not have permission to override vetoes in this guest draft."
    );
    Response(
      StatusCodes.Status404NotFound,
      "Guest draft or pick not found, or caller is not a participant."
    );
  }
}
