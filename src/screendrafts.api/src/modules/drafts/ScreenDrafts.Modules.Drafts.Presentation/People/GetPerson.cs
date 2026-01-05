namespace ScreenDrafts.Modules.Drafts.Presentation.People;

internal sealed class GetPerson(ISender sender) : Endpoint<GetPersonRequest, PersonResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/people/{id:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.People)
       .WithDescription("Get a person by ID")
       .WithName(nameof(GetPerson));
    });
    Policies(Presentation.Permissions.GetPeople);
  }

  public override async Task HandleAsync(GetPersonRequest req, CancellationToken ct)
  {
    var query = new GetPersonQuery(req.Id);
    var person = await _sender.Send(query, ct);
    await Send.OkAsync(person.Value!, ct);
  }
}

public sealed record GetPersonRequest(
  [FromRoute(Name = "id")] Guid Id);

internal sealed class GetPersonSummary : Summary<GetPerson>
{
  public GetPersonSummary()
  {
    Summary = "Get a person by ID";
    Description = "Get a person by ID. This endpoint returns the details of a person with the specified ID.";
    Response<PersonResponse>(StatusCodes.Status200OK, "Details of the person with the specified ID.");
    Response(StatusCodes.Status404NotFound, "Person not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
