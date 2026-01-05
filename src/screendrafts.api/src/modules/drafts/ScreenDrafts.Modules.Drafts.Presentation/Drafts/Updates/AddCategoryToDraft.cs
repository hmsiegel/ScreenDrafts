namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Updates;

internal sealed class AddCategoryToDraft(ISender sender) : Endpoint<AddCategoryToDraftRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/categories");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Categories)
      .WithName("AddCategoryToDraft")
      .WithDescription("Add a category to a draft");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(AddCategoryToDraftRequest req, CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");

    var command = new AddCategoryToDraftCommand(draftId, req.CategoryId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

internal sealed record AddCategoryToDraftRequest(Guid CategoryId);
  

internal sealed class AddCategoryToDraftSummary : Summary<AddCategoryToDraft>
{
  public AddCategoryToDraftSummary()
  {
    Summary = "Add a category to a draft";
    Description = "Adds a category to a draft. The category will be added to the draft.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the category added to the draft.");
    Response(StatusCodes.Status404NotFound, "Draft or category not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to add a category to this draft.");
  }
}
