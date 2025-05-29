namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class RemoveDrafterFromDraft(ISender sender) : Endpoint<RemoveDrafterFromDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Delete("/drafts/{draftId:guid}/drafter/{drafterId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithName(nameof(RemoveDrafterFromDraft))
      .WithDescription("Remove a drafter from a draft");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(RemoveDrafterFromDraftRequest req, CancellationToken ct)
  {
    var command = new RemoveDrafterFromDraftCommand(req.DraftId, req.DrafterId);
    var result = await _sender.Send(command, ct);
    await this.MapResultsAsync(result, ct);
  }
}

public sealed record RemoveDrafterFromDraftRequest(
    [FromRoute(Name = "draftId")] Guid DraftId,
    [FromRoute(Name = "drafterId")] Guid DrafterId);

internal sealed class RemoveDrafterFromDraftSummary : Summary<RemoveDrafterFromDraft>
{
  public RemoveDrafterFromDraftSummary()
  {
    Summary = "Remove a drafter from a draft";
    Description = "Remove a drafter from a draft. This endpoint allows you to remove a drafter from a specific draft by providing the draft ID and the drafter ID.";
    Response(StatusCodes.Status204NoContent, "Drafter removed successfully.");
    Response(StatusCodes.Status404NotFound, "Draft or drafter not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
