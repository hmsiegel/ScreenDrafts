namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateProfile;

internal sealed class Endpoint : ScreenDraftsEndpoint<UpdateProfileRequest>
{
  public override void Configure()
  {
    Put(PeopleRoutes.Profile);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.People)
        .WithName(DraftsOpenApi.Names.People_UpdateProfile)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PersonUpdate);
  }

  public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
  {
    var command = new UpdateProfileCommand
    {
      PublicId = req.PublicId,
      DisplayName = req.DisplayName,
      Biography = req.Biography,
      Location = req.Location,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
