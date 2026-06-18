namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
{
  public override void Configure()
  {
    Put(UserRoutes.Profile);
    Description(x =>
    {
      x.WithName(UsersOpenApi.Names.Users_UpdateUserProfile)
        .WithTags(UsersOpenApi.Tags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.UserUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var userId = User.GetUserPublicId()!;
    var command = new UpdateUserCommand(userId, req.FirstName, req.LastName, req.MiddleName);
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
