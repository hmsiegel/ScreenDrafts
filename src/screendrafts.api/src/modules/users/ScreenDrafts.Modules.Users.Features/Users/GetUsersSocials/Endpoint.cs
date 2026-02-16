namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersSocials;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, Response>
{
  public override void Configure()
  {
    Post(UserRoutes.PublicProfiles);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
      .WithName(UsersOpenApi.Names.Users_GetUsersProfiles)
      .Produces<Response>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.UsersProfiles);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new GetUsersSocialsQuery
    {
      PersonIds = req.PublicIds
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
