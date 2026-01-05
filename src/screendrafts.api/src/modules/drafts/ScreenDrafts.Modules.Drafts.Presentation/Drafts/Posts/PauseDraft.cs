namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Posts;

internal sealed class PauseDraft(ISender sender) : Endpoint<PauseDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Patch("/drafts/{draftId:guid}/pause");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithDescription("Pause a draft")
      .WithName(nameof(PauseDraft));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(PauseDraftRequest req, CancellationToken ct)
  {
    var command = new PauseDraftCommand(req.DraftId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record PauseDraftRequest(
  [FromRoute(Name = "draftId")] Guid DraftId);

internal sealed class PauseDraftSummary : Summary<PauseDraft>
{
  public PauseDraftSummary()
  {
    Summary = "Pause a draft";
    Description = "Pause a draft. This endpoint allows you to pause an ongoing draft.";
    Response(StatusCodes.Status200OK, "Draft paused successfully.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
