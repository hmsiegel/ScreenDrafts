using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetTvSeriesRestriction;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Restrict a draft to a single TV series.";
    Description =
      "Administrator-only. When set, the draft's pool, boards, and candidate lists "
      + "reject any TMDb media whose parent series doesn't match. Pass null to lift "
      + "the restriction. Intended to be set once, before candidates are added.";
    Response(StatusCodes.Status204NoContent, "Restriction updated.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
  }
}
