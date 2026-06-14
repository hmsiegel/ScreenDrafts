using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

// ── Summary ───────────────────────────────────────────────────────────────────

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get full gameplay state for a live draft part.";
    Description =
      "Returns draft positions, participant token counts, all picks with veto/override state, "
      + "trivia results, hosts, and the next expected participant. "
      + "Use on initial page load and on SignalR reconnect to re-sync state.";
    Response<GetDraftPartGameplayResponse>(200, "Gameplay state returned.");
    Response(401, "Unauthorized.");
    Response(403, "Forbidden.");
    Response(404, "Draft part not found.");
  }
}
