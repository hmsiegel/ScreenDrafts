using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateSocial;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Update a person's social handles.";
    Description =
      "Update a person's social handles with the provided information. This endpoint allows you to modify the social handles of a person in the system.";
    Response(
      StatusCodes.Status204NoContent,
      "The person's social handles were successfully updated."
    );
    Response(
      StatusCodes.Status400BadRequest,
      "The request was invalid. This can occur if the provided data is incomplete or does not meet the required format."
    );
    Response(
      StatusCodes.Status401Unauthorized,
      "Unauthorized access. This can occur if the request does not include valid authentication credentials."
    );
    Response(
      StatusCodes.Status403Forbidden,
      "Forbidden access. This can occur if the authenticated user does not have permission to update the person's profile."
    );
    Response(
      StatusCodes.Status404NotFound,
      "The specified person was not found. This can occur if the provided person ID does not exist in the system."
    );
  }
}
