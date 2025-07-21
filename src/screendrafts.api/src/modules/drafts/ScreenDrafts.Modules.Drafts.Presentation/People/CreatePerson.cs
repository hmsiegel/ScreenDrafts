namespace ScreenDrafts.Modules.Drafts.Presentation.People;

internal sealed class CreatePerson(ISender sender) : Endpoint<CreatePersonRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/people/create");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.People)
       .WithDescription("Create a new person")
       .WithName(nameof(CreatePerson));
    });
    Policies(Presentation.Permissions.CreatePeople);
  }

  public override async Task HandleAsync(CreatePersonRequest req, CancellationToken ct)
  {
    var command = new CreatePersonCommand(req.FirstName, req.LastName, req.UserId);

    var personId = await _sender.Send(command, ct);

    await this.MapResultsAsync(personId, ct);
  }
}

public sealed record CreatePersonRequest(Guid? UserId, string FirstName, string LastName);

internal sealed class CreatePersonSummary : Summary<CreatePerson>
{
  public CreatePersonSummary()
  {
    Summary = "Create a new person";
    Description = "Create a new person. This endpoint creates a new person with the specified user ID, first name, and last name.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the newly created person.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
