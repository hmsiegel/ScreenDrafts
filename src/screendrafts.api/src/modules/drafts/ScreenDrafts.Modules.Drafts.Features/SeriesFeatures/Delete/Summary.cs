using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Delete;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Soft deletes a series.";
    Description =
      "Soft deletes a series by its ID. The series will be marked as deleted but not removed from the database.";
    Response(StatusCodes.Status204NoContent, "The series was successfully soft deleted.");
    Response(
      StatusCodes.Status400BadRequest,
      "The request was invalid or cannot be otherwise served."
    );
    Response(
      StatusCodes.Status401Unauthorized,
      "The user is not authorized to perform this action."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "The user does not have permission to perform this action."
    );
    Response(StatusCodes.Status404NotFound, "The series with the specified ID was not found.");
    Response(
      StatusCodes.Status409Conflict,
      "The series could not be deleted due to a conflict with the current state of the resource."
    );
  }
}
