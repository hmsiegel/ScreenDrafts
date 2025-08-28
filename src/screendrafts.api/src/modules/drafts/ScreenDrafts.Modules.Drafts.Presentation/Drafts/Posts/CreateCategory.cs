namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Posts;

internal sealed class CreateCategory(ISender sender) : Endpoint<CreateCategoryRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/categories");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Categories)
      .WithName("AddCategory")
      .WithDescription("Add a new category");
    });
    Policies(Presentation.Permissions.CreateCategories);
  }

  public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
  {
    var command = new CreateCategoryCommand(
      Name: req.Name,
      Description: req.Description);

    var categoryId = await _sender.Send(command, ct);

    await this.MapResultsAsync(categoryId, ct);
  }
}

internal sealed record CreateCategoryRequest(
    string Name,
    string Description);

internal sealed class CreateCategorySummary : Summary<CreateCategory>
{
  public CreateCategorySummary()
  {
    Summary = "Add a new category";
    Description = "Adds a new category to the system.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the newly created category.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to create categories.");
  }
}
