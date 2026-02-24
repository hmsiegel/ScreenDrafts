using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Set the release date for a draft part.";
    Description = "Set the release date for a draft part on a specific release channel.";
    Response(StatusCodes.Status204NoContent, "Release date set successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to set the release date for this draft part.");
    Response(StatusCodes.Status404NotFound, "Draft part not found.");
  }
}
