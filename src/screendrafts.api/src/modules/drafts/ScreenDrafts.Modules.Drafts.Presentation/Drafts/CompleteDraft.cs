namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class CompleteDraft(ISender sender) : Endpoint<CompleteDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/complete");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
        .WithName(nameof(CompleteDraft))
        .WithDescription("Completes a draft. The draft will be marked as complete and cannot be modified anymore.");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(CompleteDraftRequest req, CancellationToken ct)
  {
    var command = new CompleteDraftCommand(req.DraftId);
    var result = await _sender.Send(command, ct);
    await this.MapResultsAsync(result, ct);
  }
}

public sealed record CompleteDraftRequest(
    [FromRoute(Name = "draftId")] Guid DraftId);


internal sealed class CompleteDraftSummary : Summary<CompleteDraft>
{ public CompleteDraftSummary()
  {
    Summary = "Complete a draft";
    Description = "Completes a draft. The draft will be marked as complete and cannot be modified anymore.";
    Response(StatusCodes.Status200OK, "The draft was completed successfully.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to complete this draft.");
  }
}
