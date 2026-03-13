using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "List the 10 most recent completed drafts.";
    Description = "Returns completed draft parts ordered by release date descending. " +
                        "Patreon-only drafts are included when the caller holds the patreon:search permission.";
    Response<ListLatestDraftsResponse>(StatusCodes.Status200OK, "A list of the 10 most recent completed drafts.");
  }
}
