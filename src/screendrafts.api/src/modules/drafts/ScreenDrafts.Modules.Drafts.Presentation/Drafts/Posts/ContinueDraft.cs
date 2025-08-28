namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Posts;
internal sealed class ContinueDraft(ISender sender) : Endpoint<ContinueDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Patch("/drafts/{draftId:guid}/continue");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
       .WithDescription("Continue a draft")
       .WithName(nameof(ContinueDraft));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(ContinueDraftRequest req, CancellationToken ct)
  {
    var command = new ContinueDraftCommand(req.DraftId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

internal sealed record ContinueDraftRequest(
  [FromRoute(Name = "draftId")] Guid DraftId);

internal sealed class ContinueDraftSummary : Summary<ContinueDraft>
{
  public ContinueDraftSummary()
  {
    Summary = "Continue a draft";
    Description = "Continue a draft. This endpoint allows you to continue a paused draft.";
    Response(StatusCodes.Status200OK, "Draft continued successfully.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
