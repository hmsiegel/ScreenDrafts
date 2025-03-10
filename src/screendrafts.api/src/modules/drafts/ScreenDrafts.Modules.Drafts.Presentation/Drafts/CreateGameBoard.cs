﻿namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class CreateGameBoard(ISender sender) : Endpoint<GameBoardRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/gameboard");
    Description(x => x.WithTags(Presentation.Tags.GameBoards));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(GameBoardRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new CreateGameBoardCommand(
      req.DraftId,
      req.DraftPositions);
    var result = await _sender.Send(command, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(cancellation: ct);
    }
    else
    {
      await SendOkAsync(result.Value, ct);
    }
  }
}

public sealed record GameBoardRequest(
  Guid DraftId,
  IEnumerable<DraftPositionDto>? DraftPositions = null);
