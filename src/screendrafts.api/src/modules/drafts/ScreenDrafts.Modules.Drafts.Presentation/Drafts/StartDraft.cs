namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class StartDraft(ISender sender) : Endpoint<StartDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/start");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithName("StartDraft")
      .WithDescription("Starts a draft.");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(StartDraftRequest req, CancellationToken ct)
  {
    var command = new StartDraftCommand(req.DraftId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record StartDraftRequest(
  [FromRoute(Name = "draftId")] Guid DraftId);

internal sealed class StartDraftSummary : Summary<StartDraft>
{
  public StartDraftSummary()
  {
    Summary = "Starts a draft.";
    Description = "Sets the draft status to 'In Progress' and starts the draft.";
    Response(StatusCodes.Status200OK, "Draft started successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
  }
}
