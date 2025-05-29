namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class DeleteDraft(ISender sender) : Endpoint<DeleteDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Delete("/drafts/{draftId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
       .WithDescription("Delete a draft")
       .WithName(nameof(DeleteDraft));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(DeleteDraftRequest req, CancellationToken ct)
  {
    var command = new DeleteDraftCommand(req.DraftId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record DeleteDraftRequest(
  [FromRoute] Guid DraftId);

internal sealed class DeleteDraftSummary : Summary<DeleteDraft>
{
  public DeleteDraftSummary()
  {
    Description = "Delete a draft";
    Response(StatusCodes.Status204NoContent, "The draft was successfully deleted.");
    Response(StatusCodes.Status404NotFound, "The draft was not found.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to delete this draft.");
  }
}
