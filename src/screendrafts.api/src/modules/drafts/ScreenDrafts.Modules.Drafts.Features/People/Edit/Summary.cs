namespace ScreenDrafts.Modules.Drafts.Features.People.Edit;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Edits a person.";
    Description = "Edits the details of an existing person in the system.";
    Response(StatusCodes.Status204NoContent, "The person was edited successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to edit the person.");
    Response(StatusCodes.Status404NotFound, "The person with the specified ID was not found.");
  }
}
