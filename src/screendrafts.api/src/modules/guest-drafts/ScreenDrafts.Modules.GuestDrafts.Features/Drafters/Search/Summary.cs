using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.Search;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Search for guest drafters";
    Description =
      "Live text search by display name -- backs the typeahead used to find who to add to a guest draft. Requires authentication; not a public/anonymous endpoint.";
    Response<IReadOnlyList<GuestDrafterSummaryResponse>>(
      StatusCodes.Status200OK,
      "Matching guest drafters, capped at 50 results."
    );
    Response(StatusCodes.Status403Forbidden, "Not authenticated.");
  }
}
