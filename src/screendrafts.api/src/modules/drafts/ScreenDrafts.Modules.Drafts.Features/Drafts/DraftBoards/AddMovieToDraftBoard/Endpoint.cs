namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.AddMovieToDraftBoard;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddMovieToDraftBoardRequest>
{
  public override void Configure()
  {
    Post(DraftRoutes.Board);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftBoards)
      .WithName(DraftsOpenApi.Names.DraftBoards_AddItem)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftBoardCreate);
  }

  public override async Task HandleAsync(AddMovieToDraftBoardRequest req, CancellationToken ct)
  {
    var userPublicId = User.GetPublicId() ?? string.Empty;

    var command = new AddMovieToDraftBoardCommand
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
