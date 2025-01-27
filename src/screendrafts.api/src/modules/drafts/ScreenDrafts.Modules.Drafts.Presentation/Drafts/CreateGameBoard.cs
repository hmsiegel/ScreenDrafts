namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class CreateGameBoard(ISender sender) : Endpoint<GameBoardRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/gameboards");
    Description(x => x.WithTags(Presentation.Tags.GameBoards));
    AllowAnonymous();
  }

  public override async Task HandleAsync(GameBoardRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new CreateGameBoardCommand(
      req.DraftId,
      req.DraftType,
      req.DraftPositions);
    var result = await _sender.Send(command, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(cancellation: ct);
    }
    else
    {
      await SendOkAsync(ct);
    }
  }
}

public sealed record GameBoardRequest(
  Guid DraftId,
  string DraftType,
  IEnumerable<DraftPositionDto> DraftPositions);
