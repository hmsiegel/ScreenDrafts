using ScreenDrafts.Common.Features.Http.Responses;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new campaign";
    Description = "Creates a new campaign with the specified parameters.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The PublicId of the created campaign.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create a campaign.");
  }
}


