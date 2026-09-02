using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVeto;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Veto a pick";
    Description =
      "The caller vetoes as themselves. May only be applied to the most recently played pick, regardless of that pick's veto state.";
    Response(StatusCodes.Status204NoContent, "Veto applied.");
    Response(
      StatusCodes.Status400BadRequest,
      "Not the most recent pick, already vetoed, or the caller has no remaining vetoes."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "You do not have permission to veto picks in this guest draft."
    );
    Response(
      StatusCodes.Status404NotFound,
      "Guest draft or pick not found, or caller is not a participant."
    );
  }
}
