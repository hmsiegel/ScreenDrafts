using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetEpisodeNumber;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set the episode number for a draft's release channel.";
    Description = "Sets the episode number for a specific release channel associated with the draft. If the release channel does not exist, it will be created.";
    Response(StatusCodes.Status204NoContent, "Episode number set successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request data.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized access.");
    Response(StatusCodes.Status403Forbidden, "Forbidden access.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
  }
}
