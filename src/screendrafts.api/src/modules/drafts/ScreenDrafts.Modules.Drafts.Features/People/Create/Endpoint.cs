using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreatePersonRequest, string>
{
  public override void Configure()
  {
    Post(PeopleRoutes.People);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.People)
       .WithName(DraftsOpenApi.Names.People_CreatePerson)
       .Produces<string>(StatusCodes.Status201Created)
       .Produces(StatusCodes.Status400BadRequest)
       .Produces(StatusCodes.Status401Unauthorized)
       .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(DraftsAuth.Permissions.PersonCreate);
  }

  public override async Task HandleAsync(CreatePersonRequest req, CancellationToken ct)
  {
    var CreatePersonCommand = new CreatePersonCommand
    {
      UserId = req.UserId,
      FirstName = req.FirstName,
      LastName = req.LastName,
      PublicId = req.PublicId
    };

    var result = await Sender.Send(CreatePersonCommand, ct);

    await this.SendCreatedAsync(
      result.Map(id => id.ToString()),
      PeopleLocations.ById,
      ct);
  }
}


