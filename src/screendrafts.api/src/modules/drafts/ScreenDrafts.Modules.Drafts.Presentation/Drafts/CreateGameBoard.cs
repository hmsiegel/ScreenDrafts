namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class CreateGameBoard(ISender sender) : Endpoint<CreateGameBoardRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/gameboard/{draftId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.GameBoards)
      .WithDescription("Creates a game board for the draft.")
      .WithName(nameof(CreateGameBoard));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(CreateGameBoardRequest req, CancellationToken ct)
  {
    var command = new CreateGameBoardCommand(
      req.DraftId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record CreateGameBoardRequest(
    [FromRoute(Name = "draftId")] Guid DraftId);

internal sealed class CreateGameBoardSummary : Summary<CreateGameBoard>
{
  public CreateGameBoardSummary()
  {
    Summary = "Create a game board for a draft";
    Description = "Creates a game board for the draft.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the game board created.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create a game board for this draft.");
  }
}
