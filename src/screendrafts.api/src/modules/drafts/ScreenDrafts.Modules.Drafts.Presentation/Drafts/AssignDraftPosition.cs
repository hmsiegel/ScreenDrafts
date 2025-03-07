namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class AssignDraftPosition(ISender sender) : Endpoint<AssignDraftPositionRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/position/{drafterId:guid}");
    Description(x => x. WithTags(Presentation.Tags.DraftPositions));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(AssignDraftPositionRequest req, CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var drafterId = Route<Guid>("drafterId");

    var command = new AssignDraftPositionCommand(
      draftId,
      drafterId,
      req.PositionId);

    var result = await _sender.Send(command, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(ct);
    }
  }
}

public sealed record AssignDraftPositionRequest(Guid PositionId);
