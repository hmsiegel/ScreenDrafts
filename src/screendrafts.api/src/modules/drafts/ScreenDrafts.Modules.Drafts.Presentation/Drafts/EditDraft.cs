namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class EditDraft(ISender sender) : Endpoint<EditDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Put("/drafts/{draftId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
           .WithDescription("Edit a draft")
           .WithName(nameof(EditDraft));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(EditDraftRequest req, CancellationToken ct)
  {
    var command = new EditDraftCommand(
      req.DraftId,
      req.Title,
      DraftType.FromName(req.DraftType),
      req.TotalPicks,
      req.TotalDrafters,
      req.TotalDrafterTeams,
      req.TotalHosts,
      EpisodeType.FromName(req.EpisodeType),
      DraftStatus.FromName(req.DraftStatus));

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record EditDraftRequest(
  [FromRoute(Name = "draftId")] Guid DraftId,
  string Title,
  string DraftType,
  int TotalPicks,
  int TotalDrafters,
  int TotalDrafterTeams,
  int TotalHosts,
  string EpisodeType,
  string DraftStatus);

internal sealed class EditDraftSummary : Summary<EditDraft>
{
  public EditDraftSummary()
  {
    Description = "Edit a draft";
    Response<Guid>(StatusCodes.Status200OK, "The draft was successfully edited.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status404NotFound, "The draft was not found.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to edit this draft.");
  }
}
