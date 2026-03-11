using FastEndpoints;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get the current user's draft board for a draft.";
    Description = "Retrieves the draft board associated with the current user for a specific draft, including the movies added to the board.";
    Response<GetDraftBoardResponse>(StatusCodes.Status200OK, "The draft board was successfully retrieved.");
    Response(StatusCodes.Status404NotFound, "The user was not found or does not have a draft board for the specified draft.");
  }
}
