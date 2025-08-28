namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Posts;

internal sealed class AddPick(ISender sender) : Endpoint<AddPickRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/picks");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Picks)
      .WithName("AddPick")
      .WithDescription("Add a pick to a draft");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(AddPickRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new AddPickCommand(
      DraftId: req.DraftId,
      Position: req.Position,
      MovieId: req.MovieId,
      DrafterId: req.DrafterId,
      DrafterTeamId: req.DrafterTeamId,
      PlayOrder: req.PlayOrder);

    var pickId = await _sender.Send(command, ct);

    await this.MapResultsAsync(pickId, ct);
  }
}
public sealed record AddPickRequest(
    [FromRoute(Name = "draftId")] Guid DraftId,
    Guid? DrafterId,
    Guid? DrafterTeamId,
    int Position,
    int PlayOrder,
    Guid MovieId);

internal sealed class AddPickSummary : Summary<AddPick>
{
  public AddPickSummary()
  {
    Summary = "Add a pick to a draft";
    Description = "Adds a pick to a draft. The pick will be added to the draft at the specified position.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the pick added to the draft.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to add a pick to this draft.");
  }
}

