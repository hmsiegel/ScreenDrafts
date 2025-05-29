namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class RemoveHostFromDraft(ISender sender) : Endpoint<RemoveHostFromDraftRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Delete("/drafts/{draftId:guid}/remove-host/{hostId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithName(nameof(RemoveHostFromDraft))
      .WithDescription("Remove a host from a draft");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(RemoveHostFromDraftRequest req, CancellationToken ct)
  {
    var command = new RemoveHostFromDraftCommand(req.DraftId, req.HostId);
    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record RemoveHostFromDraftRequest(
    [FromRoute(Name = "draftId")] Guid DraftId,
    [FromRoute(Name = "hostId")] Guid HostId);

internal sealed class RemoveHostFromDraftSummary : Summary<RemoveHostFromDraft>
{
  public RemoveHostFromDraftSummary()
  {
    Summary = "Remove a host from a draft";
    Description = "Remove a host from a draft. This endpoint allows you to remove a host from a specific draft.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the draft from which the host was removed.");
    Response(StatusCodes.Status404NotFound, "Draft or host not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
