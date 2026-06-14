using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

// ── Summary ───────────────────────────────────────────────────────────────────

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Undo a veto on a pick. Commissioner-only / break-glass.";
    Description =
      "Reverses a veto by play order. Restores the pick to the active board "
      + "and refunds the veto token to the original issuer. "
      + "Cannot undo a veto that has already been overridden.";
    Response(204, "Veto reversed.");
    Response(400, "Validation error or veto already overridden.");
    Response(401, "Unauthorized.");
    Response(403, "Forbidden.");
    Response(404, "Draft part or pick not found.");
  }
}
