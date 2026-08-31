using FastEndpoints;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Create a new guest draft";
    Description = "Creates a new private guest draft owned by the specified user.";
    Response<CreatedResponse>(StatusCodes.Status201Created, "The PublicId of the created guest draft.");
    Response(StatusCodes.Status400BadRequest, "Invalid CreateGuestDraftRequest.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create a guest draft.");
  }
}
