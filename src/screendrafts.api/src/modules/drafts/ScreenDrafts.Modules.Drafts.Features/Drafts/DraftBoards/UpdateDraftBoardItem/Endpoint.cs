namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.UpdateDraftBoardItem;

internal sealed class Endpoint : ScreenDraftsEndpoint<UpdateDraftBoardItemRequest>
{
  public override void Configure()
  {
    Put(DraftRoutes.BoardItem);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftBoards)
      .WithName(DraftsOpenApi.Names.DraftBoards_UpdateItem)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftBoardCreate);
  }

  public override async Task HandleAsync(UpdateDraftBoardItemRequest req, CancellationToken ct)
  {
    var userPublicId = User.GetPublicId() ?? string.Empty;

    var command = new UpdateDraftBoardItemCommand
    {
      DraftId = req.DraftId,
      UserPublicId = userPublicId,
      TmdbId = req.TmdbId,
      Notes = req.Notes,
      Priority = req.Priority
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
