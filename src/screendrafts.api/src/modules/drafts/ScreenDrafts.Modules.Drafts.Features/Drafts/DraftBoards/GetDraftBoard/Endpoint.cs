namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.GetDraftBoard;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDraftBoardRequest, GetDraftBoardResponse>
{
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
    var userId = User.GetUserId();

    var query = new GetDraftBoardQuery { DraftId = req.PublicId, UserId = userId };
    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
