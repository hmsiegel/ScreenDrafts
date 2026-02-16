using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "List People";
    Description = "Get a list of people.";
    Response<PeopleCollectionResponse>(StatusCodes.Status200OK, "A list of people.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
