namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.BulkAddMoviesToDraftBoard;

internal sealed class Endpoint : ScreenDraftsEndpoint<BulkAddMoviesToDraftBoardRequest, BulkAddMoviesResponse>
{
  public override void Configure()
  {
    Post(DraftRoutes.BoardBulk);
    AllowFileUploads();
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftBoards)
      .WithName(DraftsOpenApi.Names.DraftBoards_BulkAddItems)
      .Produces<BulkAddMoviesResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftBoardCreate);
  }

  public override async Task HandleAsync(BulkAddMoviesToDraftBoardRequest req, CancellationToken ct)
  {
    var userPublicId = User.GetPublicId()!;

    using var stream = req.File.OpenReadStream();

    var command = new BulkAddMoviesToDraftBoardCommand
    {
      DraftId = req.DraftId,
      UserPublicId = userPublicId,
      CsvStream = stream
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
