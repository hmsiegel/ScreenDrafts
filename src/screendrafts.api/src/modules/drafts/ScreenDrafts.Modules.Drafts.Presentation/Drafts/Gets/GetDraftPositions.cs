namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Gets;

internal sealed class GetDraftPositions(ISender sender) : Endpoint<GetDraftPositionsRequest, List<DraftPositionResponse>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/{gameboardId:guid}/positions");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.DraftPositions)
      .WithDescription("Get all draft positions for a gameboard")
      .WithName(nameof(GetDraftPositions));
    });
    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(GetDraftPositionsRequest req, CancellationToken ct)
  {
    var query = new GetDraftPositionsByGameBoardQuery(req.GameboardId);

    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      var positionList = result.Value.ToList();
      await Send.OkAsync(positionList, ct);
    }
  }
}

public sealed record GetDraftPositionsRequest(
    [FromRoute(Name = "gameboardId")] Guid GameboardId);

internal sealed class GetDraftPositionsSummary : Summary<GetDraftPositions>
{ public GetDraftPositionsSummary()
  {
    Summary = "Get all draft positions for a gameboard";
    Description = "Get all draft positions for a gameboard. This endpoint returns a list of draft positions for the specified gameboard.";
    Response<List<DraftPositionResponse>>(StatusCodes.Status200OK, "List of draft positions for the specified gameboard.");
    Response(StatusCodes.Status404NotFound, "Gameboard not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
