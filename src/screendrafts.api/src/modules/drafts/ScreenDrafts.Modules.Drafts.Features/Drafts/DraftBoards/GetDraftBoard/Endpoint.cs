using Microsoft.AspNetCore.Http.HttpResults;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

internal sealed class Endpoint(IUsersApi usersApi)
  : ScreenDraftsEndpoint<GetDraftBoardRequest, GetDraftBoardResponse>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Get(DraftRoutes.Board);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftBoards)
      .WithName(DraftsOpenApi.Names.DraftBoards_GetBoard)
      .Produces<GetDraftBoardResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftBoardRead);
  }

  public override async Task HandleAsync(GetDraftBoardRequest req, CancellationToken ct)
  {
    var user = await _usersApi.GetUserByPublicId(User.GetPublicId(), ct);

    if (user is null)
    {
      await Send.NotFoundAsync(ct);
      return;
    }
    var query = new GetDraftBoardQuery
    {
      DraftId = req.DraftId,
      UserId = user.UserId
    };
    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
