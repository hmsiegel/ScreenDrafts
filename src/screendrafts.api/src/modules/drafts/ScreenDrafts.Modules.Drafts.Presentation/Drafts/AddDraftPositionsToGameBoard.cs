namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

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
      .WithDescription("Add Draft Positions to GameBoard");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(AddDraftPositionsToGameBoardRequest req, CancellationToken ct)
  {
    var gameBoardId = Route<Guid>("gameBoardId");

    var command = new AddDraftPositionsToGameBoardCommand(
      gameBoardId,
      req.DraftPositions);

    var response = await _sender.Send(command, ct);

    await this.MapResultsAsync(response, ct);
  }
}

public sealed record AddDraftPositionsToGameBoardRequest(Collection<DraftPositionRequest> DraftPositions);
