using ScreenDrafts.Common.Features.Http.Responses;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new draft";
    Description = "Creates a new draft with the specified parameters.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The PublicId of the created draft.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create a draft.");
  }
}
