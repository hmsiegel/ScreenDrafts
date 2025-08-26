namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class UpdateScreamDrafts(ISender sender) : Endpoint<UpdateScreamDraftsRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Patch("/drafts/{draftId:guid}/screamdrafts");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithDescription("Update the scream drafts status of a draft")
      .WithName(nameof(UpdateScreamDrafts));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(UpdateScreamDraftsRequest req, CancellationToken ct)
  {
    var command = new UpdateScreamDraftsCommand(req.DraftId, req.IsScreamDrafts);
    var result = await _sender.Send(command, ct);

    if (result.IsFailure)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await Send.NoContentAsync(ct);
    }
  }
}

public sealed record UpdateScreamDraftsRequest(
    bool IsScreamDrafts,
    [FromRoute(Name = "draftId")] Guid DraftId);

internal sealed class UpdateScreamDraftsSummary : Summary<UpdateScreamDrafts>
{
  public UpdateScreamDraftsSummary()
  {
    Summary = "Update the scream drafts status of a draft";
    Description = "Update the scream drafts status of a draft. This endpoint allows you to update whether a specific draft is a scream drafts or not.";
    Response(StatusCodes.Status204NoContent, "Scream drafts status updated successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
