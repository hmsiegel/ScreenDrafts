namespace ScreenDrafts.Modules.Drafts.Features.People.Get;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Gets a person by their Public Id.";
    Description = "Retrieves the details of a person using their Public Id.";
    Response<PersonResponse>(StatusCodes.Status200OK, "Details of the person with the specified Public Id.");
    Response(StatusCodes.Status404NotFound, "Person not found.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized to access this resource.");
    Response(StatusCodes.Status403Forbidden, "Forbidden to access this resource.");
  }
}
