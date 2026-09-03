using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

// ── Summary ───────────────────────────────────────────────────────────────

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get full gameplay state for a guest draft.";
    Description =
      "Returns board positions, participant token balances, and every pick with its "
      + "veto/override/reveal state. Unrevealed picks conceal their movie from anyone "
      + "who isn't the picker, the designated revealer, or the owner. Only visible to "
      + "the draft's owner or participants -- returns 404 for anyone else, to avoid "
      + "confirming a private draft's existence. Use on initial page load and on "
      + "SignalR reconnect to re-sync state.";
    Response<GetGuestDraftGameplayResponse>(StatusCodes.Status200OK, "Gameplay state returned.");
    Response(StatusCodes.Status403Forbidden, "Not authenticated.");
    Response(
      StatusCodes.Status404NotFound,
      "Guest draft not found, or caller is not the owner or a participant."
    );
  }
}
