using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get a draft by its public ID.";
    Description = "Gets the details of a draft, including its parts and associated data. Requires 'draft:read' permission.";
    Response<GetDraftResponse>(StatusCodes.Status200OK, "The draft was found and returned successfully.");
    Response(StatusCodes.Status404NotFound, "No draft with the specified public ID was found.");
  }
}
