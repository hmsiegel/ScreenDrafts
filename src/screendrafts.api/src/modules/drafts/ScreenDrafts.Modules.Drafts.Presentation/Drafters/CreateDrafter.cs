namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class CreateDrafter(ISender sender) : EndpointWithoutRequest<Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafters/create/{id:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafters)
      .WithDescription("Create a new drafter")
      .WithName(nameof(CreateDrafter));
    });
    Policies(Presentation.Permissions.CreateDrafter);
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var personId = Route<Guid>("id");
    var command = new CreateDrafterCommand(personId);

    var drafterId = await _sender.Send(command, ct);

    await this.MapResultsAsync(drafterId, ct);
  }
}


internal sealed class CreateDrafterSummary : Summary<CreateDrafter>
{
  public CreateDrafterSummary()
  {
    Summary = "Create a new drafter";
    Description = "Create a new drafter. This endpoint creates a new drafter with the specified user ID and name.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the newly created drafter.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
