namespace ScreenDrafts.Modules.Drafts.Presentation._Legacy.Drafts.Posts;

internal sealed class AddDrafterToDraft(ISender sender) : Endpoint<AddDrafterToDraftRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/drafters/{drafterId:guid}");
    Description(x =>
    {
      x.WithTags(_Legacy.Tags.Drafts)
      .WithName(nameof(AddDrafterToDraft))
      .WithDescription("Add Drafter to Draft");
    });
    Policies(_Legacy.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(AddDrafterToDraftRequest req, CancellationToken ct)
  {
    var command = new AddDrafterToDraftCommand(req.DraftId, req.DrafterId);
    var result = await _sender.Send(command, ct);
    await this.MapResultsAsync(result, ct);
  }
}

public sealed record AddDrafterToDraftRequest(
    [FromRoute(Name = "draftId")] Guid DraftId,
    [FromRoute(Name = "drafterId")] Guid DrafterId);


internal sealed class AddDrafterToDraftSummary : Summary<AddDrafterToDraft>
{
  public AddDrafterToDraftSummary()
  {
    Summary = "Add Drafter to Draft";
    Description = "Add Drafter to Draft";
    Response<Guid>(StatusCodes.Status200OK, "Drafter added to draft successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status404NotFound, "Draft or Drafter not found.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to add a drafter to this draft.");
  }
}
