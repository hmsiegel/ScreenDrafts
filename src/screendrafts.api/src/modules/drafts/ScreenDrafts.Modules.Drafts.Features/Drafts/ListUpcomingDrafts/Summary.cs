using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a list of upcoming and in-progress drafts.";
    Description = "Returns draft parts that have not yet completed, ordered by scheduled release date. " +
                     "Each entry includes the caller's capabilities for that draft (edit, start, join, etc.) " +
                     "derived from their role.";
    Response<ListUpcomingDraftsResponse>(StatusCodes.Status200OK, "A list of upcoming and in-progress drafts.");
  }
}
