using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AddSubDraft;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Add a sub-draft to a speed draft part.";
    Description = "Adds a new sub-draft to the specified draft part at the given index.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The sub-draft was successfully created.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to add a sub-draft.");
    Response(StatusCodes.Status404NotFound, "The specified draft part was not found.");
  }
}
