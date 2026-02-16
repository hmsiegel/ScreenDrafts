namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed class Endpoint : ScreenDraftsEndpoint<LinkUserPersonRequest>
{
  public override void Configure()
  {
    Post(PeopleRoutes.LinkUser);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.People_LinkUser)
      .WithTags(DraftsOpenApi.Tags.People)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.PersonUpdate);
  }

  public override async Task HandleAsync(LinkUserPersonRequest req, CancellationToken ct)
  {
    var LinkUserPersonCommand = new LinkUserPersonCommand
    {
      PublicId = req.PublicId,
      UserId = req.UserId
    };

    var result = await Sender.Send(LinkUserPersonCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}


