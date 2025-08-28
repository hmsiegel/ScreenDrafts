namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Posts;

internal sealed class AddDraftPositionsToGameBoard(ISender sender)
  : Endpoint<AddDraftPositionsToGameBoardRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("drafts/{gameBoardId:guid}/positions");
    Description(x =>
    {
      x.WithTags("Drafts")
      .WithName(nameof(AddDraftPositionsToGameBoard))
      .WithDescription("Add Draft Positions to GameBoard");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(AddDraftPositionsToGameBoardRequest req, CancellationToken ct)
  {
    var command = new AddDraftPositionsToGameBoardCommand(
      req.GameBoardId,
      req.DraftPositions);

    var response = await _sender.Send(command, ct);

    await this.MapResultsAsync(response, ct);
  }
}
public sealed record AddDraftPositionsToGameBoardRequest(
    [FromRoute(Name = "gameBoardId")] Guid GameBoardId,
    Collection<DraftPositionRequest> DraftPositions);


internal sealed class AddDraftPositionsToGameBoardSummary : Summary<AddDraftPositionsToGameBoard>
{
  public AddDraftPositionsToGameBoardSummary()
  {
    Summary = "Add Draft Positions to GameBoard";
    Description = "Add Draft Positions to GameBoard";
    Response(200, "Draft positions added successfully.");
    Response<ErrorResponse>(400, "Bad Request");
    Response<ErrorResponse>(401, "Unauthorized");
    Response<ErrorResponse>(403, "Forbidden");
    Response<ErrorResponse>(404, "Not Found");
  }
}
