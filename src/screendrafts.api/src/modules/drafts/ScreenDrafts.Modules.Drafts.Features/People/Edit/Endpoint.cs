namespace ScreenDrafts.Modules.Drafts.Features.People.Edit;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
{
  public override void Configure()
  {
    Put(PeopleRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.People_EditPerson)
      .WithTags(DraftsOpenApi.Tags.People)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Permissions(Features.Permissions.PersonUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      PublicId = req.PublicId,
      FirstName = req.FirstName,
      LastName = req.LastName,
      DisplayName = req.DisplayName
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
