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
    var userId = User.GetPublicId()!;
    var command = new Command(
      userId,
      req.FirstName,
      req.LastName,
      req.MiddleName,
      req.ProfilePicture,
      req.TwitterHandle,
      req.InstagramHandle,
      req.LetterboxdHandle,
      req.BlueskyHandle);
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
