using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new host";
    Description = "Create a new host";
    Response<string>(StatusCodes.Status201Created, "The ID of the created host.");
    Response(StatusCodes.Status400BadRequest, "Invalid CreateHostRequest.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
    Response(StatusCodes.Status403Forbidden, "Forbidden.");
  }
}

