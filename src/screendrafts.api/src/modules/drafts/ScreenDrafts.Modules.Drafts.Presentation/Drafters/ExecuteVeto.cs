namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class ExecuteVeto(ISender sender) : Endpoint<ExecuteVetoRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/veto/{pickId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafters)
      .WithName(nameof(ExecuteVeto))
      .WithDescription("Execute a veto on a pick in a draft");
    });
    Policies(Presentation.Permissions.VetoPicks);
  }
  public override async Task HandleAsync(ExecuteVetoRequest req, CancellationToken ct)
  {
    var command = new ExecuteVetoCommand(req.DrafterTeamId, req.DrafterId, req.PickId, req.DraftId);
    var result = await _sender.Send(command, ct);
    await this.MapResultsAsync(result, ct);
  }
}

public sealed record ExecuteVetoRequest(
  Guid? DrafterId,
  Guid? DrafterTeamId,
  [FromRoute(Name = "draftId")] Guid DraftId,
  [FromRoute(Name = "pickId")] Guid PickId);

internal sealed class ExecuteVetoSummary : Summary<ExecuteVeto>
{
  public ExecuteVetoSummary()
  {
    Summary = "Execute a veto on a pick in a draft";
    Description = "Execute a veto on a pick in a draft. This endpoint allows a drafter to veto a pick in the specified draft.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the vetoed pick.");
    Response(StatusCodes.Status404NotFound, "Draft or pick not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
