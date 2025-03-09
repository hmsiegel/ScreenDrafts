namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class GetDraftPositions(ISender sender) : EndpointWithoutRequest<List<DraftPositionResponse>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/{gameboardId:guid}/positions");
    Description(x => x.WithTags(Presentation.Tags.DraftPositions));
    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var gameboardId = Route<Guid>("draftId");

    var query = new GetDraftPositionsByGameBoardQuery(gameboardId);

    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      var positionList = result.Value.ToList();
      await SendOkAsync(positionList, ct);
    }
  }
}
