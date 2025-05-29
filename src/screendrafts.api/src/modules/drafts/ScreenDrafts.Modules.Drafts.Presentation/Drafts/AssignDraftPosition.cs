namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class AssignDraftPosition(ISender sender) : Endpoint<AssignDraftPositionRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/position/{drafterId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.DraftPositions)
        .WithName(nameof(AssignDraftPosition))
        .WithDescription("Assign a position to a drafter in a draft.")
        .Produces<Guid>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(AssignDraftPositionRequest req, CancellationToken ct)
  {
    var command = new AssignDraftPositionCommand(
      req.DraftId,
      req.DrafterId,
      req.PositionId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record AssignDraftPositionRequest(
  Guid PositionId,
  [FromRoute(Name = "draftId")] Guid DraftId,
  [FromRoute(Name = "drafterId")] Guid DrafterId);

internal sealed class AssignDraftPositionSummary : Summary<AssignDraftPosition>
{
  public AssignDraftPositionSummary()
  {
    Summary = "Assign a position to a drafter";
    Description = "Assigns a position to a drafter in a draft. The drafter will be able to pick from the assigned position.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the position assigned to the drafter.");
    Response(StatusCodes.Status404NotFound, "Draft or position not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to assign a position to this drafter.");
  }
}
