namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class CreateGameBoard(ISender sender) : EndpointWithoutRequest<Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/gameboard/{draftId:guid}");
    Description(x => x.WithTags(Presentation.Tags.GameBoards));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var command = new CreateGameBoardCommand(
      draftId);

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

